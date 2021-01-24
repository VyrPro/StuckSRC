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
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;

namespace StuckSrc
{
   
public partial class Form1 : Form
    {
        public static List<string> ArchList = new List<string>();
        public static List<string> ErrList = new List<string>();

        public static List<string> PageList = new List<string>();
        public static List<string> MsgList = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void populateDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread pThread = new Thread(PopulateDB);
            pThread.Start();
        }
        /*private void PopulateDB()
        {
            int count = 1;
            int page = 3595;
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=StuckSource.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM raw_pages";
                object res = null;
                try
                {
                    res = cmd.ExecuteScalar();
                } catch (Exception ex)
                {
                    
                }
                if (res == null)
                {
                    cmd.CommandText = "CREATE TABLE raw_pages (id INTEGER PRIMARY KEY AUTOINCREMENT, page LONGTEXT)";
                    cmd.ExecuteNonQuery();
                }
            }
            while (page < 8130)
            {
                while (count < 10)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            ArchList.Add(wc.DownloadString("https://www.homestuck.com/story/" + page));
                        }
                    } catch(Exception ex)
                    {
                        Console.WriteLine("404 at: " + page);
                    }
                    count++;
                    page++;
                }
                foreach(string s in ArchList)
                {
                    using (SQLiteConnection conn = new SQLiteConnection("Data Source=StuckSource.db"))
                    {
                        conn.Open();
                        SQLiteCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO raw_pages (page) VALUES (@p)";
                        cmd.Parameters.AddWithValue("@p", s);
                        cmd.ExecuteNonQuery();
                    }
                }
                MethodInvoker del = delegate { progressBar1.Value += 10; };
                progressBar1.Invoke(del);
                ArchList.Clear();
                count = 0;
                Thread.Sleep(500);
            }
        }*/
        private void PopulateDB()
        {

        }
        private void processDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = 1;
            List<string> RunList = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=StuckSource.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                //8129
                while (count < 8129)
                {
                    cmd.CommandText = "SELECT * FROM raw_pages WHERE id=@i";
                    cmd.Parameters.AddWithValue("@i", count);
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {

                            string Raw = rdr[1].ToString();


                            string[] RawSplit = Raw.Split(new[] { '\r', '\n' });
                            foreach (string s in RawSplit)
                            {
                                if (s.Contains("<span ") && !s.Contains("&copy; 2018 Homestuck") && !s.Contains("<span class=\"\">&gt;</span>"))
                                {
                                    string rX = Regex.Replace(s, "<.*?>", String.Empty);
                                    RunList.Add(rX);
                                }
                            }
                        }
                    }
                    count++;
                }
            }
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=StuckSource.db"))
            {
                conn.Open();
                foreach (string s in RunList)
                {
                    SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO raw_msg (msg)VALUES(@m)";
                    cmd.Parameters.AddWithValue("@m", s);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // toolStrip1.Renderer = new MySR();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=StuckSource.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM raw_msg";
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        MsgList.Add(rdr[1].ToString());
                    }
                }
                cmd.CommandText = "SELECT * FROM raw_pages";
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        PageList.Add(rdr[1].ToString());
                    }
                }
            }
        }
        private void Search()
        {
            if (!textBox1.Text.Contains("||"))
            {
                foreach (string s in MsgList)
                {
                    if (checkBox1.Checked)
                    {
                        if (s.ToLower().Contains(textBox1.Text.ToLower()))
                        {
                            MethodInvoker del = delegate { listBox1.Items.Add(s); };
                            listBox1.Invoke(del);

                        }
                    }
                    else
                    {
                        if (s.Contains(textBox1.Text))
                        {
                            MethodInvoker del = delegate { listBox1.Items.Add(s); };
                            listBox1.Invoke(del);

                        }
                    }
                    MethodInvoker ael = delegate { progressBar1.Value += 1; };
                    progressBar1.Invoke(ael);
                }
            } else
            {
                string[] sString = new string[] { "||" };
                string[] MultiOptions = textBox1.Text.Split(sString, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in MsgList)
                {
                    if (checkBox1.Checked)
                    {
                        if (s.ToLower().Contains(MultiOptions[0]) || s.ToLower().Contains(MultiOptions[1]))
                        {
                            MethodInvoker del = delegate { listBox1.Items.Add(s); };
                            listBox1.Invoke(del);

                        }
                    }
                    else
                    {
                        if (s.Contains(MultiOptions[0]) || s.ToLower().Contains(MultiOptions[1]))
                        {
                            MethodInvoker del = delegate { listBox1.Items.Add(s); };
                            listBox1.Invoke(del);

                        }
                    }
                    MethodInvoker ael = delegate { progressBar1.Value += 1; };
                    progressBar1.Invoke(ael);
                }
            }
            MethodInvoker cel = delegate { label1.Text = "Results: " + listBox1.Items.Count; };
            label1.Invoke(cel);
            MethodInvoker mel = delegate { button1.Enabled = true; };
            button1.Invoke(mel);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            label1.Text = "Searching...";
            button1.Enabled = false;
            listBox1.Items.Clear();
            Thread search = new Thread(Search);
            search.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ReturnPages = string.Empty;
            if(listBox1.SelectedItem != null)
            {
                foreach(string s in PageList)
                {
                    if(s.Contains(listBox1.SelectedItem.ToString()))
                    {
                        string[] RawSplit = s.Split(new[] { '\r', '\n' });
                        foreach (string sr in RawSplit)
                        {
                            if (sr.Contains("<link ") && sr.Contains("/story/"))
                            {
                                string[] srb = new string[] { "story/" };
                                ReturnPages += sr.Split(srb, StringSplitOptions.RemoveEmptyEntries)[1].Split('\"')[0] + ", ";
                            }
                        }
                    }
                }
                MessageBox.Show(ReturnPages);
            }
           
        }

        private void cBox(object sender, EventArgs e)
        {
            if (textBox2.Text != listBox1.SelectedIndex.ToString())
            {
                textBox2.Text = listBox1.SelectedItem.ToString();
            }
        }
    }
    public class ToolStripOverride : ToolStripProfessionalRenderer
    {
        public ToolStripOverride() { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
    }
}
