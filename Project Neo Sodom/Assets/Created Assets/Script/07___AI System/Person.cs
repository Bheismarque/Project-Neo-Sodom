using System;
using System.Collections.Generic;

namespace AI_System
{
    public class Desire
    {

    }
    public class Person
    {
        private List<Desire> desires = new List<Desire>();
        private List<Statement> knowledges = new List<Statement>();

        public void thinkAbout(Statement situation)
        {
            // Step 1 - Result Prediction : Expect What is Going to Happen After the Situation
            foreach (Statement knowledge in knowledges)
            {
            }

            // Step 2 - Emotion Analysis : Feel the Emotion Caused by the Results

            // Step 3 - Planning
        }
    }
}