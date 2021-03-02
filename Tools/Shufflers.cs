using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vkaudioposter_Console.Tools
{
    class Shufflers
    {
        public static List<string> ShufflePlaylist(List<string> inputList)
        {
            List<string> shuffled = new List<string>();

            var rnd = new Random();
            shuffled = inputList.OrderBy(item => rnd.Next()).ToList<string>();

            return shuffled;
        }

        public static List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }
    }
}
