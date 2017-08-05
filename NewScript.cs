using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZS_Express
{
    public partial class NewScript : Form
    {

        public string ScriptType = null;
        public string ScriptName = "NewScript";
        public bool isZh;


        public NewScript()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(radioButton1.Checked == true)
            {
                ScriptType = "item";
                isZh = false;
            } else if (radioButton2.Checked == true)
            {
                ScriptType = "FFC";
                isZh = false;
            } else if (radioButton3.Checked == true)
            {
                ScriptType = "Global";
                isZh = false;
            } else if (radioButton4.Checked == true)
            {
                ScriptType = "Header";
                isZh = true;
            }

            ScriptName = textBox1.Text;
            this.Hide();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (isZh == false)
            {
                label3.Text = textBox1.Text + ".z";
            } else
            {
                label3.Text = textBox1.Text + ".zh";
            }
        }
    }
}
