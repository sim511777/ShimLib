using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShimLibTest {
    public partial class RadioListBoxTest : UserControl {
        public RadioListBoxTest() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            numericUpDown1.Value = radioListBox1.SelectedIndex;
        }

        private void button2_Click(object sender, EventArgs e) {
            radioListBox1.SelectedIndex = (int)numericUpDown1.Value;
        }
    }
}
