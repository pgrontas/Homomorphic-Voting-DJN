using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

using System.Data.SQLite;
using CryptoLib;
using Microsoft.FSharp.Core;

namespace ElectionCreator
{
    public partial class frmElection : Form
    {
        public frmElection()
        {
            InitializeComponent();
        }

        void log(string msg)
        {
            txtLog.AppendText(msg + "\n");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                initDB();
                var eid = insertElection();
                log("Created Election" + eid);
                var cnds = insertElectionOptions(eid);
                log("Created Election Options For " + eid);
                insertKey(eid,cnds);
                log("Success");
            }
            catch (Exception ex)
            {
                log(ex.ToString());
            }
           
        }

        SQLiteConnection dbConn;
        private void initDB()
        {
            var connString = @"Data Source=C:\Users\Panagiotis\Dropbox\Universities\ΜΠΛΑ\Διπλωματική\Code\DJN\djn.db;Version=3;";
            dbConn = new SQLiteConnection(connString);
            dbConn.Open();
        }

        private Guid insertElection()
        {
            var desc = txtElectionName.Text.Trim();
            var cmd = dbConn.CreateCommand();
            var eID = System.Guid.NewGuid();
            cmd.CommandText = " INSERT INTO Elections (ID, Description) " +
                              " VALUES (@ID, @Desc) ";
            cmd.Parameters.AddWithValue("@ID", eID);
            cmd.Parameters.AddWithValue("@Desc", desc);
       
            cmd.ExecuteNonQuery();

            return eID;
        }

        private int insertElectionOptions(Guid eid)
        {
            var cmd = dbConn.CreateCommand();
            var options = txtElectionOptions.Text.Trim();
            var c = Environment.NewLine + "\t";
            var ls = options.Split(c.ToCharArray());
            int i = 0;
            foreach (var l in ls)
            {
                if (l==string.Empty) continue;
                var opt = l.Split( new char[]{' '} );
                cmd.CommandText = " INSERT INTO ElectionOptions (ID, Label, Value, ElectionID) " +
                              " VALUES (@ID, @Label, @Value, @ElectionID) ";
                cmd.Parameters.AddWithValue("@ID", System.Guid.NewGuid());
                cmd.Parameters.AddWithValue("@Label", opt[0]);
                cmd.Parameters.AddWithValue("@Value", Int32.Parse(opt[1]));
                cmd.Parameters.AddWithValue("@ElectionID", eid.ToString());
                cmd.ExecuteNonQuery();
                i++;
            }
            return i;
        }

        private void insertKey(Guid eid,int cnds)
        {
            var k = Int32.Parse(txtLength.Text.Trim());
            var s = Int32.Parse(txtS.Text.Trim());
            var n = Int32.Parse(txtN.Text.Trim());
            var t = Int32.Parse(txtT.Text.Trim());
            
            var cr = new Paillier.ThresPaillier(t, n);
            var keys = cr.KeyShareGen(k, s);
            var pk = keys.Item1;
            var secretKeys = keys.Item2;
            int i=1;
            foreach (var sk in secretKeys) {
                System.IO.File.WriteAllLines("sk" + i + ".txt", new string[] { sk.ToString() });
                //log("Secret Key="+sk.ToString());
                i++;
            }

            var M = 103;

            var cmd = dbConn.CreateCommand();
            cmd.CommandText = " INSERT INTO PublicKeys (ID, n, s, D, ssp, Type, numShares, numThreshold, M, ElectionID) " +
                              " VALUES (@ID, @n, @s,@D, @ssp, @Type, @ns, @nt, @M, @ElectionID) ";
            cmd.Parameters.AddWithValue("@ID", System.Guid.NewGuid());
            cmd.Parameters.AddWithValue("@n", pk.Item1);
            cmd.Parameters.AddWithValue("@s", pk.Item2);
            cmd.Parameters.AddWithValue("@D", pk.Item3);
            cmd.Parameters.AddWithValue("@ssp", pk.Item4);
            cmd.Parameters.AddWithValue("@ns", n);
            cmd.Parameters.AddWithValue("@nt", t);
             cmd.Parameters.AddWithValue("@M", M);
            cmd.Parameters.AddWithValue("@Type", 0);
            cmd.Parameters.AddWithValue("@ElectionID", eid.ToString());
            cmd.ExecuteNonQuery();

        }
    }
}
