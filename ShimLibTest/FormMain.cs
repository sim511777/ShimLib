using ShimLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimLibTest {
    public partial class FormMain : Form {
        public FormMain(string[] args) {
            InitializeComponent();
            imageBoxTest1.LoadCommandLine(args);
        }
    }
}
