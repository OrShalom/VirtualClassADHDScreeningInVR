using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Model
{
    public class Report
    {
        public DateTime Time { get; set; }
        public string PatientId { get; set; }
        public List<float> PressedAndshould { get; set; }
        public List<float> PressedAndshouldNot { get; set; }
        public List<float> NotPressedAndshould { get; set; }
        public List<Vector> HeadRotation { get; set; }
    }

    public class Vector
    {
        public float x;
        public float y;
        public float z;
        public Vector(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }
}
