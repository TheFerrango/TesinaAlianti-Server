﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Data.SqlClient;
using System.Data;

/// <summary>
/// Summary description for dbInteraction
/// </summary>
public class dbInteraction
{
  string _connStr;
  SqlConnection _sqlConn;

  public dbInteraction()
  {
    _connStr = (string.Format("Server={0}; Database={1}; User ID={2}; Password={3}", @".\sqlexpress", "DBTraini", "coop", "rosso"));
  }

  private bool Connect()
  {
    try
    {
      _sqlConn = new SqlConnection(_connStr);
      _sqlConn.Open();
    }
    catch
    {
      return false;
    }
    return true;
  }

  private void Close()
  {
    _sqlConn.Close();
  }

  #region metodi Sync

  public int AddPilot(Pilota pilo)
  {
    Connect();
    int id = ChkPilot(pilo, "Trainatore");
    if (id != -1)
      return id;

    SqlCommand sqlComm = new SqlCommand("insertPilot", _sqlConn);
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;

    sqlComm.Parameters.Add("nome", pilo.nome);
    sqlComm.Parameters.Add("cognome", pilo.cognome);
    sqlComm.ExecuteNonQuery();
    id = ChkPilot(pilo, "Trainatore");
    Close();
    return id;
  }  

  public int AddPilotAliante(Pilota pilo)
  {
    Connect();
    int id = ChkPilot(pilo, "Pilota");
    if (id != -1)
      return id;

    SqlCommand sqlComm = new SqlCommand("insertPilotAliante", _sqlConn);
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;

    sqlComm.Parameters.Add("nome", pilo.nome);
    sqlComm.Parameters.Add("cognome", pilo.cognome);
    sqlComm.ExecuteNonQuery();
    id = ChkPilot(pilo, "Pilota");
    Close();
    return id;
  }

  public int AddInstructor(Pilota pilo)
  {
    Connect();
    int id = ChkPilot(pilo, "Istruttore");
    if (id != -1)
      return id; 
    SqlCommand sqlComm = new SqlCommand("insertIstruttore", _sqlConn);
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;

    sqlComm.Parameters.Add("nome", pilo.nome);
    sqlComm.Parameters.Add("cognome", pilo.cognome);
    sqlComm.ExecuteNonQuery();
    id = ChkPilot(pilo, "Istruttore");
    Close();
    return id;
  }

  public int AddModAliante(ModelloAereo mod)
  {
    Connect();
    int id = ChkPlane(mod, "ModelloAliante");
    if (id != -1)
      return id;
    SqlCommand sqlComm = new SqlCommand("insertModelloAliante", _sqlConn);
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;
    sqlComm.Parameters.Add("codice", mod.code);

    sqlComm.Parameters.Add("modello", mod.model);

    sqlComm.ExecuteNonQuery();
    id = ChkPlane(mod, "ModelloAliante");
    Close();
    return id;
  }

  public int AddModTrainatore(ModelloAereo mod)
  {
    Connect();
    
    int id = ChkPlane(mod, "ModelloTrainatore");
    if (id != -1)
      return id;

    SqlCommand sqlComm = new SqlCommand("insertModelloTrainatore", _sqlConn);
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;

    sqlComm.Parameters.Add("codice", mod.code);
    sqlComm.Parameters.Add("modello", mod.model);
    sqlComm.ExecuteNonQuery();
    id = ChkPlane(mod, "ModelloTrainatore");
    Close();
    return id;
  }

  

  public int AddGiornata(SessioneGiorno sg)
  {
    Connect();

    SqlCommand sqlComm = new SqlCommand("insertGiornata", _sqlConn);
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;
    Log(sg.Data);
    SqlParameter sqlP = new SqlParameter("data", System.Data.SqlDbType.DateTime);
    Log(fixaDate(sg.Data));
    sqlP.Value = Convert.ToDateTime(fixaDate(sg.Data));
    sqlComm.Parameters.Add(sqlP);
    sqlComm.Parameters.Add("unita", sg.UniOrametro);
    sqlComm.Parameters.Add("idTraino", sg.IDTrainatore);
    sqlComm.Parameters.Add("idModTraino", sg.IDModTrainatore);
    sqlComm.ExecuteNonQuery();

    sqlComm = new SqlCommand("select ID from Giornata order by id desc", _sqlConn);
    SqlDataReader sqr = sqlComm.ExecuteReader();
    sqr.Read();
    int c = Convert.ToInt32(sqr[0]);
    sqr.Close();
    Close();
    return c;
  }

  public int AddTraino(Traini tr)
  {
    Connect();

    SqlCommand sqlComm = new SqlCommand("insertTraino", _sqlConn);
    Log("coop");
    sqlComm.CommandType = System.Data.CommandType.StoredProcedure;
    sqlComm.Parameters.Add("idGiorno", tr.IDGiornata);
    sqlComm.Parameters.Add("idIstr", tr.IDIstruttore);
    sqlComm.Parameters.Add("idAlia", tr.IDAliante);
    sqlComm.Parameters.Add("idPilo", tr.IDPilota);
    sqlComm.Parameters.Add("tempo", tr.TempoOrametro);
    sqlComm.ExecuteNonQuery();
    Close();
    return -1;
  }

  private int ChkPilot(Pilota pilo, string tabella)
  {
    SqlCommand sqlComm = new SqlCommand("Select * from " + tabella + " where nome like '" + pilo.nome + "' and cognome like '" + pilo.cognome + "'", _sqlConn);
    SqlDataReader sqr = sqlComm.ExecuteReader();

    if (sqr.HasRows)
    {
      sqr.Read();
      int id = (int)sqr[0];
      sqr.Close();
      return id;
    }
    sqr.Close();
    return -1;
  }

  private int ChkPlane(ModelloAereo mod, string tabella)
  {
    SqlCommand sqlComm = new SqlCommand("Select * from " + tabella + " where Codice like '" + mod.code + "'", _sqlConn);
    SqlDataReader sqr = sqlComm.ExecuteReader();

    if (sqr.HasRows)
    {
      sqr.Read();
      int id = (int)sqr[0];
      sqr.Close();
      return id;
    }
    sqr.Close();
    return -1;
  }

  public string fixaDate(string data)
  {
    string[] vetS = new string[] { "gen", "feb", "mar", "apr", "mag", "giu", "lug", "ago", "set", "ott", "nov", "dec" };
    string[] c = data.Split('/');
    for (int i = 0; i < vetS.Length; i++)
    {
      if (vetS[i] == c[1])
      {
        if (i < 9)
        {
          int s = i + 1;
          c[1] = "0" + s.ToString();
        }
        else
          c[1] = (i + 1).ToString();
      }
    }
    return string.Format("{0}/{1}/{2}", c[2].Split(' ')[0], c[1], c[0]);
  }

  #endregion

  public bool ExecuteLogin(string usr, string pwd)
  {
    Connect();
    SqlCommand sqlComm = new SqlCommand("Select * from Users where usrName like '" + usr + "' and usrPwd like '" + pwd + "'", _sqlConn);
    SqlDataReader sqr = sqlComm.ExecuteReader();
    bool found = sqr.HasRows;
    sqr.Close();
    Close();
    return found;
  }

  public DataTable RetrievePiloti(string table)
  {
    Connect();
    DataTable dt = new DataTable();
    SqlCommand sqlComm = new SqlCommand("Select ID, Nome, Cognome FROM " + table, _sqlConn);
    dt.Load(sqlComm.ExecuteReader());
    Close();
    return dt;
  }

  public DataTable RetrieveAereo(string table)
  {
    Connect();
    DataTable dt = new DataTable();
    SqlCommand sqlComm = new SqlCommand("Select Codice, Modello FROM " + table, _sqlConn);
    dt.Load(sqlComm.ExecuteReader());
    Close();
    return dt;
  }

  public DataTable RetrieveSessioni(int idTraino)
  {
    Connect();
    DataTable dt = new DataTable();
    SqlCommand sqlComm = new SqlCommand("Select ID, Data FROM Giornata where IDTrainatore = " + idTraino, _sqlConn);
    dt.Load(sqlComm.ExecuteReader());
    Close();
    return dt;
  }

  public DataTable RetrieveTraini(int sessione)
  {
    Connect();
    DataTable dt = new DataTable();
    SqlCommand sqlComm = new SqlCommand("ottieniTraini", _sqlConn);
    sqlComm.CommandType = CommandType.StoredProcedure;
    sqlComm.Parameters.Add("idGiornata", sessione.ToString());
    dt.Load(sqlComm.ExecuteReader());
    Close();
    return dt;
  }

  public void Log(string text)
  {
    File.AppendAllText(@"C:\MS-DOS\coop.txt", text + Environment.NewLine);
  }
}