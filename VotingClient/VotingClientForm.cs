using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SQLite;
using System.Numerics;
using CryptoLib;
using Microsoft.FSharp.Core;

namespace VotingClient
{
    public partial class VotingClientForm : Form
    {
        private VotingClient vc;
        private int vote;
        public VotingClientForm()
        {
            InitializeComponent();
            var actLog = new Action<String>(log);
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

        private void fillElectionOptions()
        {
            var eDict = vc.retrieveElectionOptions();
            cmbElectionOptions.DisplayMember = "Key";
            cmbElectionOptions.ValueMember = "Value";
            cmbElectionOptions.DataSource = new BindingSource(eDict, null);

        }

        void log(string msg)
        {
            txtLog.AppendText(msg + "\n");
        }   

        private void btnCastVote_Click(object sender, EventArgs e)
        {
            var c = vc.doEncrypt(vote);
            vc.doInsertVote(c);
        } 
        
        private void cmbElection_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (cmbElection.SelectedItem != null)
            {
                vc.ElectionID = cmbElection.SelectedValue.ToString();
                fillElectionOptions();
            }
        }

        private void cmbElectionOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbElectionOptions.SelectedItem != null)
            {
                vote = (int)(cmbElectionOptions.SelectedValue);
            }
        }

        private void VotingClientForm_Load(object sender, EventArgs e)
        {

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

        public Dictionary<string, int> retrieveElectionOptions()
        {
            string sql = "Select label,value from ElectionOptions where ElectionID = @Eid ";
            SQLiteCommand command = new SQLiteCommand(sql, dbConn);
            command.Parameters.AddWithValue("Eid", electionID);
            SQLiteDataReader reader = command.ExecuteReader();
            var eDict = new Dictionary<string, int>();

            while (reader.Read())
            {
                eDict.Add(reader["label"].ToString(), Int32.Parse(reader["value"].ToString()));
            }
            log("Successfully Retrieved Election Options For Election " + ElectionID);

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
            var s= Int32.Parse(reader["s"].ToString()); 
            log("Successfully Retrieved Public Key!" + n + "," + s);

            var pk = Tuple.Create(n, s, new BigInteger(1), new BigInteger(1));
            return pk;

        }

        public BigInteger doEncrypt(int vote)
        { 
            var pk = retrieveKey();
            var M = retrieveM();
            var eg = new DJN.DJN(M);
            var c = eg.mvote<BigInteger, BigInteger>(vote, pk);
            log("Cipher" + c.ToString());
            return c;
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

        public void doInsertVote(BigInteger c)
        {
            SQLiteCommand insertSQL = new SQLiteCommand(
                    "INSERT INTO Votes (Id, Cipher, ElectionID) VALUES (@id,@C,@Eid)", dbConn);
            insertSQL.Parameters.AddWithValue("@id", System.Guid.NewGuid());
            insertSQL.Parameters.AddWithValue("@C", c.ToString()); 
            insertSQL.Parameters.AddWithValue("@Eid", electionID);
            try
            {
                insertSQL.ExecuteNonQuery();
                log("Vote Cast" + c.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        } 

    }
}
