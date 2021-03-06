﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Data.SQLite;
using CryptoLib;
using System.Numerics;

using Proof = System.Tuple<System.Numerics.BigInteger[], System.Numerics.BigInteger, System.Numerics.BigInteger[], System.Numerics.BigInteger[]>;
using CipherProof = System.Tuple<System.Numerics.BigInteger, System.Tuple<System.Numerics.BigInteger[], System.Numerics.BigInteger, System.Numerics.BigInteger[], System.Numerics.BigInteger[]>>;

namespace Tallier
{
    public partial class frmTallier : Form
    {
        private VotingClient vc;
        public frmTallier()
        {
            InitializeComponent(); var actLog = new Action<String>(log);
            vc = new VotingClient(actLog);
            fillCmbElections();
        }
        private void fillCmbElections()
        {
            var eDict = vc.retrieveElections();
            cmbElection.DisplayMember = "Value";
            cmbElection.ValueMember = "Key";
            cmbElection.DataSource = new BindingSource(eDict, null);
        }

        void log(string msg)
        {
            txtLog.AppendText(msg + "\n");
        }

        private void cmbElection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbElection.SelectedItem != null)
            {
                vc.ElectionID = cmbElection.SelectedValue.ToString();
            }
        }

        private void btnAggregate_Click(object sender, EventArgs e)
        {
            vc.aggregate();
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] lines=System.IO.File.ReadAllLines(openFileDialog1.FileName);
                var l = lines[0].Split(new char[]{'(',',',')'});
                var sk = Tuple.Create(Int32.Parse(l[1]), BigInteger.Parse(l[2]));
                vc.decryptShare(sk);  
            }
            
        }

        private void btnCombine_Click(object sender, EventArgs e)
        {
            vc.combineShares();
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            vc.checkProofs();
        }

        
    }
}

class VotingClient
{
    private string electionID;

    public string ElectionID
    {
        get { return electionID; }
        set { electionID = value; }
    }

    private SQLiteConnection dbConn;
    private Action<string> log;

    public VotingClient(Action<string> log)
    {
        initDB();
        this.log = log;
        this.electionID = string.Empty;
    }


    private void initDB()
    {
        var connString = @"Data Source=C:\Users\Panagiotis\Dropbox\Universities\ΜΠΛΑ\Διπλωματική\Code\DJN\djn.db;Version=3;";
        dbConn = new SQLiteConnection(connString);
        dbConn.Open();

    }

    public Dictionary<string, string> retrieveElections()
    {

        string sql = "select * from elections";
        SQLiteCommand command = new SQLiteCommand(sql, dbConn);
        SQLiteDataReader reader = command.ExecuteReader();
        var eDict = new Dictionary<string, string>();

        while (reader.Read())
        {
            eDict.Add(reader["id"].ToString(), reader["description"].ToString());
        }
        log("Successfully Retrieved Elections!");

        return eDict;


    }

    public Tuple<BigInteger, int, BigInteger, BigInteger> retrieveKey()
    {
        string sql = "select n,s,D,ssp from PublicKeys where type = 0 and ElectionID = @Eid";
        SQLiteCommand command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();
        var n = BigInteger.Parse(reader["n"].ToString());
        var s = Int32.Parse(reader["s"].ToString());
        var D = BigInteger.Parse(reader["D"].ToString());
        var ssp = BigInteger.Parse(reader["ssp"].ToString());
        log("Successfully Retrieved Public Key! ");

        var pk = Tuple.Create(n, s, D, ssp);
        return pk;

    }

    public Tuple<int, int> retrieveThreshold()
    {
        string sql = "select numShares,numThreshold from PublicKeys where ElectionID = @Eid";
        SQLiteCommand command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();
        var n = Int32.Parse(reader["n"].ToString());
        var t = Int32.Parse(reader["s"].ToString());
        log("Successfully Retrieved Threshold Values " + n + "," + t);

        var th = Tuple.Create(n, t);
        return th;
    }

    internal List<BigInteger> retrieveValidVotes()
    {
        var sql = "select Cipher from Votes where electionID = @Eid and isValid = 1";
        var command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var reader = command.ExecuteReader();
        var ls = new List<BigInteger>();
        while (reader.Read())
        {
            var t = BigInteger.Parse(reader["Cipher"].ToString());
            ls.Add(t);
        }
        log("Retrieved " + ls.Count + " Votes");
        return ls;
    }

    internal List<string> retrieveVoteIDs()
    {
        var sql = "select id from Votes where electionID = @Eid";
        var command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var reader = command.ExecuteReader();
        var ls = new List<string>();
        while (reader.Read())
        {
            var t =  reader["id"].ToString();
            ls.Add(t);
        }
        log("Retrieved " + ls.Count + " Vote Ids");
        return ls;
    }

    internal BigInteger retrieveResultCipher()
    {
        var sql = "select ResultCipher from Result where electionID = @Eid and isShare = 0";
        var command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var res = command.ExecuteScalar();
        return BigInteger.Parse(res.ToString());
    }
    internal List<Tuple<BigInteger, BigInteger>> retrieveDecryptedShares()
    {
        var sql = "select ShareNo,ResultCipher from Result where electionID = @Eid and isShare = 1";
        var command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var reader = command.ExecuteReader();
        var ls = new List<Tuple<BigInteger, BigInteger>>();
        while (reader.Read())
        {
            var i = BigInteger.Parse(reader["ShareNo"].ToString());
            var t = BigInteger.Parse(reader["ResultCipher"].ToString());
            ls.Add(Tuple.Create(i, t));
        }
        log("Retrieved " + ls.Count + " Shares");
        return ls;
    }
     
    internal BigInteger aggregate()
    {
        var ls = retrieveValidVotes();         
        var pk = retrieveKey();
        var nc = retrieveTotalCandidates();
        var djn = new DJN.DJN(nc);
        var c = djn.aggregate(ls, pk);

        log("Aggregated to " + c.ToString());
        addResult(c);
        return c;
    }
    internal void decrypt()
    {
        var pk = retrieveKey();
        var nc = retrieveTotalCandidates();
        var c = aggregate();

        var djn = new DJN.DJN(nc);
        string[] lines = System.IO.File.ReadAllLines(@"sk.txt");
        var sk = BigInteger.Parse(lines[0]);
        var res = djn.decrypt(c, pk, sk);
        foreach (var b in res)
            log(b.ToString());
    } 
    internal void addResult(BigInteger c)
    {
        var cmd = dbConn.CreateCommand();
        cmd.CommandText = " INSERT INTO Result (ID, ElectionID, ResultCipher, isShare) " +
                          " VALUES (@ID, @ElectionID, @ResultCipher, @isShare) ";
        cmd.Parameters.AddWithValue("@ID", System.Guid.NewGuid());
        cmd.Parameters.AddWithValue("@ElectionID", electionID.ToString());
        cmd.Parameters.AddWithValue("@ResultCipher", c.ToString());
        cmd.Parameters.AddWithValue("@isShare", 0);
        cmd.ExecuteNonQuery();
        log("Added Result To DB "); 
    }

    internal void decryptShare(Tuple<int,BigInteger> sk)
    {
        var c = retrieveResultCipher();
        var pk = retrieveKey();
        var share = DJN.DJN.decryptShare(c, pk, sk);
        log("Decrypted Share!");
        addDecryptedShare(share);
    }

    internal void addDecryptedShare(Tuple<int, BigInteger> share)
    {
        var cmd = dbConn.CreateCommand();
        cmd.CommandText = " INSERT INTO Result (ID, ElectionID, ResultCipher, isShare, ShareNo) " +
                          " VALUES (@ID, @ElectionID, @ResultCipher, @isShare, @shareNo) ";
        cmd.Parameters.AddWithValue("@ID", System.Guid.NewGuid());
        cmd.Parameters.AddWithValue("@ElectionID", electionID.ToString());
        cmd.Parameters.AddWithValue("@ResultCipher", share.Item2.ToString());
        cmd.Parameters.AddWithValue("@isShare", 1);
        cmd.Parameters.AddWithValue("@shareNo", share.Item1.ToString());
        cmd.ExecuteNonQuery();
        log("Added Decrypted Share To DB"); 
    }

    private int retrieveTotalCandidates()
    {
        string sql = "select count(*) from ElectionOptions where ElectionID = @Eid";
        SQLiteCommand command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var m = command.ExecuteScalar();
        log("Successfully Retrieved M=" + m);

        return Int32.Parse(m.ToString());
    }

    private Dictionary<int, string> retrieveElectionOptions()
    {
        string sql = "select label,value from ElectionOptions where ElectionID = @Eid";
        SQLiteCommand command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var reader = command.ExecuteReader();
        var ret = new Dictionary<int, string>();
        while (reader.Read())
        {
            var i = reader["label"].ToString();
            var t = Int32.Parse(reader["value"].ToString());
            ret.Add(t, i);
        }
        return ret;
    }

    private BigInteger retrieveM()
    {
        string sql = "select M from PublicKeys where type = 0 and ElectionID = @Eid";
        SQLiteCommand command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        var m = command.ExecuteScalar();
        log("Successfully Retrieved M=" + m);

        return BigInteger.Parse(m.ToString());
    }
    internal void combineShares()
    {
        var allShares = retrieveDecryptedShares();
        var pk = retrieveKey();
        var m = retrieveM();
        //var nc = retrieveTotalCandidates();
        var options = retrieveElectionOptions();
        var nc = options.Count;
        var djn = new DJN.DJN(nc);
        var allres = djn.combine(allShares.ToArray(), pk);
        for (int i = 0; i < allres.Length; i++)
        {
            var label = options[i];
            var res = allres[i];
            log("Result for " + label +" is " + res);
        } 
    } 
    internal void checkProofs()
    {
        var nc = retrieveTotalCandidates();
        var pk = retrieveKey();
        var djn = new DJN.DJN(nc);
        var vids = retrieveVoteIDs();
        int count = 0;
        foreach (var vid in vids)
        {
            var vote = retrieveProofs(vid);
            var ok = false;
            if (vote.Item2.Item1.Length > 0)
            {
                ok = djn.checkProof(vote.Item1, vote.Item2, pk);
                if (ok) count++;
                setVoteValidity(vid, ok);
            }
            log("Vote with id:" + vid + (ok ? " is valid!" : " is invalid!"));
        }
        log("Num of valid votes = "+count);
 
    }

    private void setVoteValidity(string vid, bool ok)
    {
        var cmd = dbConn.CreateCommand();
        cmd.CommandText = @" UPDATE VOTES SET ISVALID = @ISV WHERE ID=@VID AND ELECTIONID=@EID";

        cmd.Parameters.AddWithValue("@ISV", ok?1:0);
        cmd.Parameters.AddWithValue("@VID", vid);
        cmd.Parameters.AddWithValue("@EID", electionID.ToString()); 
        cmd.ExecuteNonQuery();
        log("Updated Validity Status For:"+ vid); 
    }

    private CipherProof retrieveProofs(string voteID)
    {
        var sql = @"select voteid,cipher,offer,challenge,zs,cs 
                    from VoteProofs vp, Votes v
                    where vp.electionID = @Eid and vp.voteid = @vid
                    and vp.voteID = v.id";
        var command = new SQLiteCommand(sql, dbConn);
        command.Parameters.AddWithValue("@Eid", electionID);
        command.Parameters.AddWithValue("@vid", voteID);
        var reader = command.ExecuteReader();

        var offers = new List<BigInteger>();
        var zs = new List<BigInteger>();
        var cs = new List<BigInteger>();
        var c = new BigInteger();
        var cipher = new BigInteger();
        while (reader.Read())
        {
            var tmp = BigInteger.Parse(reader["offer"].ToString());
            offers.Add(tmp);
            tmp = BigInteger.Parse(reader["zs"].ToString());
            zs.Add(tmp);
            tmp = BigInteger.Parse(reader["cs"].ToString());
            cs.Add(tmp);
            c = BigInteger.Parse(reader["challenge"].ToString());
            cipher = BigInteger.Parse(reader["cipher"].ToString());
        }
        log("Retrieved " + offers.Count + " Proofs");
        var pr = Tuple.Create(offers.ToArray(), c, zs.ToArray(), cs.ToArray());
        var ret = Tuple.Create(cipher, pr);
        return ret;
    }
}