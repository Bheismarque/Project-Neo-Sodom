using System;
using System.Collections.Generic;

namespace AI_System
{
    public class Statement
    {
        public List<Description> conditions = new List<Description>();
        public List<Description> outcomes = new List<Description>();
    }

    public class Description
    {
        public Time when;
        public Being where;
        public Being subjective;
        public Action does;
        public Being objective;
    }

    public class Time
    {
        private bool isRelative = false;
    }

    public class Being
    {
        public enum relativePosition
        {
            At,
            In, Out,
            Above, Below, NextTo, InFrontOf, BehindOf,
            On, Beneath, RightSideOf, LeftSideOf, FrontSideOf, BackSideOf
        }
        public relativePosition relativeTo;
    }

    public class Action
    {
    }
}
