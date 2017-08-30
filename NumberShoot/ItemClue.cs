using UnityEngine;
using System;
using System.Collections.Generic;

namespace NumberShoot
{
    public class ItemClue : AimTargetObject
    {
        public TextMesh textMesh;

        private int number;

        public int Number
        {
            get { return number; }
            set
            { 
                number = value; 
                textMesh.text = number.ToString();
                gameObject.SetActive(number > 0);
            }
        }
    }
}
