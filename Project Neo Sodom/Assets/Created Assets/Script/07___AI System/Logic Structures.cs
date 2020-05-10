using System.Collections.Generic;

namespace AI_System
{
    public class Statement
    {
        public List<Description> conditions = new List<Description>();
        public List<Description> outcomes = new List<Description>();
    }

    public interface Description
    {
    }

    public class Description_State : Description
    {
        private Being Subject;
        private StateDescription Verb;
        private Being Property;
    }

    public class Description_Action : Description
    {
        private Being Subject;
        private Action Verb;
        private Being Object;
    }

    public class Description_Service : Description
    {
        private Being Subject;
        private Action Service;
        private Being IndirectObject;
        private Being DirectObject;
    }

    public class Description_Affect : Description
    {
        private Being Subject;
        private Action Affect;
        private Description ObjectAffect;
    }

    public class Time
    {
        private bool isRelative = false;
    }

    public class Being
    {
    }

    public enum StateDescription
    {
        Be,
        Feel, Taste, Smell, Sound, Look
    }

    public class Action
    {
    }
}
