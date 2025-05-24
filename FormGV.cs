using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTL
{
    public partial class FormGV : Form
    {
        private readonly string maGV;
        public FormGV(string maGV)
        {
            InitializeComponent();
            this.maGV = maGV;
        }
    }
}
