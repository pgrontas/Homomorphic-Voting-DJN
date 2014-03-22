namespace VotingClient
{
    partial class VotingClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.cmbElection = new System.Windows.Forms.ComboBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCastVote = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbElectionOptions = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Select Elections";
            // 
            // cmbElection
            // 
            this.cmbElection.FormattingEnabled = true;
            this.cmbElection.Location = new System.Drawing.Point(101, 12);
            this.cmbElection.Name = "cmbElection";
            this.cmbElection.Size = new System.Drawing.Size(171, 21);
            this.cmbElection.TabIndex = 8;
            this.cmbElection.SelectedIndexChanged += new System.EventHandler(this.cmbElection_SelectedIndexChanged_1);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 108);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(260, 141);
            this.txtLog.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Select Vote";
            // 
            // btnCastVote
            // 
            this.btnCastVote.Location = new System.Drawing.Point(197, 79);
            this.btnCastVote.Name = "btnCastVote";
            this.btnCastVote.Size = new System.Drawing.Size(75, 23);
            this.btnCastVote.TabIndex = 12;
            this.btnCastVote.Text = "Cast Vote";
            this.btnCastVote.UseVisualStyleBackColor = true;
            this.btnCastVote.Click += new System.EventHandler(this.btnCastVote_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Action Log";
            // 
            // cmbElectionOptions
            // 
            this.cmbElectionOptions.FormattingEnabled = true;
            this.cmbElectionOptions.Items.AddRange(new object[] {
            "Yes",
            "No"});
            this.cmbElectionOptions.Location = new System.Drawing.Point(101, 43);
            this.cmbElectionOptions.Name = "cmbElectionOptions";
            this.cmbElectionOptions.Size = new System.Drawing.Size(171, 21);
            this.cmbElectionOptions.TabIndex = 14;
            this.cmbElectionOptions.SelectedIndexChanged += new System.EventHandler(this.cmbElectionOptions_SelectedIndexChanged);
            // 
            // VotingClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.cmbElectionOptions);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCastVote);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbElection);
            this.Controls.Add(this.txtLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VotingClientForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Voting Client";
            this.Load += new System.EventHandler(this.VotingClientForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbElection;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCastVote;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbElectionOptions;
    }
}

