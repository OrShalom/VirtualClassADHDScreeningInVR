using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model
{
    public class Session
    {
        public char[] lettersDataList { get; set; }
        public List<float> PressedAndshould { get; set; }
        public List<float> PressedAndshouldNot { get; set; }
        public List<float> NotPressedAndshould { get; set; }
        public List<Vector> HeadRotation { get; set; }
    }
}
