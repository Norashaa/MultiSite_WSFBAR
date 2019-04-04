using System;
using System.Collections.Generic;

namespace Broadcom.Tests
{
    abstract class Multisite<T> where T : Test
    {
        protected List<T> sites = new List<T>();

        public Multisite() { }

        /// <summary>
        /// Creates a new multisite test object from existing single site tests.
        /// </summary>
        /// <param name="tests">Array of niTest objects.</param>
        public Multisite(T[] tests)
        {
            foreach (T test in tests)
                sites.Add(test);
        }

        public void AddSite(T test)
        {
            if (sites.Exists(o => o.Equals(test)))
                throw new ArgumentException("Cannot have the same test object in the multisite test.");
            sites.Add(test);
        }

        public bool RemoveSite(T test)
        {
            return sites.Remove(test);
        }

        /// <summary>
        /// Returns the number of sites.
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            return sites.Count;
        }

        public void InitializeAll()
        {
            sites.ForEach(test => test.Initialize());
        }

        public void ConfigureAll()
        {
            sites.ForEach(test => test.Configure());
        }

        public void InitiateAll()
        {
            sites.ForEach(test => test.Initiate());
        }

        public void WaitAll()
        {
            sites.ForEach(test => test.Wait());
        }

        public dynamic[] FetchAll()
        {
            dynamic[] results = new dynamic[sites.Count];
            for(int i = 0; i < sites.Count; i++)
            {
                results[i] = sites[i].Fetch();
            }
            return results;
        }

        public void CloseAll()
        {
            sites.ForEach(test => test.Close());
        }
    }
}
