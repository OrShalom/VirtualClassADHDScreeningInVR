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
        public List<int> PressedAndshould { get; set; }
        public List<int> PressedAndshouldNot { get; set; }
        public List<int> NotPressedAndshould { get; set; }
        public List<List<int>> TimesOfShouldPress { get; set; }
        public List<Vector> HeadRotation { get; set; }
    }
}
