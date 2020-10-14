﻿using System.Collections.Generic;
using System.Linq;

namespace Translation.Interpolation
{
    public static class Simplifier
    {
        // Метод Куайна-МакКласки
        public static HashSet<uint> GetPrimeImplicants(List<uint> terms, int numBits)
        {
            bool done = false;
            var marked = new HashSet<uint>();

            while (!done)
            {
                var groups = new Dictionary<int, HashSet<uint>>();

                foreach (uint t in terms)
                {
                    int numOnes = Bit.OnesCount(t);
                    if (!groups.ContainsKey(numOnes))
                    {
                        groups[numOnes] = new HashSet<uint>();
                    }
                    groups[numOnes].Add(t);
                }

                terms = new List<uint>();
                var used = new HashSet<uint>();

                foreach (int key in groups.Keys)
                {
                    int key_next = key + 1;
                    if (groups.ContainsKey(key_next))
                    {
                        var group_next = groups[key_next];
                        foreach (uint t1 in groups[key])
                        {
                            uint one = 1;
                            uint lastOne = 1u << numBits;
                            uint maskOne = lastOne;
                            for (; one < lastOne; one <<= 1, maskOne <<= 1)
                            {
                                if ((t1 & (one | maskOne)) == 0)
                                {
                                    uint t2 = t1 | one;

                                    if (group_next.Contains(t2))
                                    {
                                        uint t12 = t1 | maskOne;

                                        terms.Add(t12);
                                        used.Add(t1);
                                        used.Add(t2);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var g in groups.Values)
                {
                    g.ExceptWith(used);
                    marked.UnionWith(g);
                }

                if (used.Count == 0)
                {
                    done = true;
                }
            }

            return marked;
        }

        public static IEnumerable<uint> GetEssentialImplicants(List<uint> essentialTerms, IEnumerable<uint> implicants, int numBits)
        {
            foreach (uint implicant in implicants)
            {
                uint mask = ~(implicant >> numBits) & Bit.Ones(numBits);
                foreach (uint term in essentialTerms)
                {
                    if ((term & mask) == (implicant & mask))
                    {
                        yield return implicant;
                        break;
                    }
                }
            }
        }

        public static IEnumerable<uint> Permutations(uint value, int numBits)
        {
            uint baseTerm = Bit.GetFirst(value, numBits);
            uint mask = Bit.GetRange(value, numBits, numBits);
            uint variant = 0;
            int variantLength = Bit.OnesCount(mask);
            if (variantLength == 0)
            {
                yield return baseTerm;
                yield break;
            }

            uint maxVariant = Bit.MaxNum(variantLength);

            for (;variant <= maxVariant; variant++)
            {
                uint variantOne = 1;
                uint lastVariantOne = 1u << variantLength;
                uint maskOne = 1;
                uint premutation = baseTerm;
                do
                {
                    if ((mask & maskOne) != 0)
                    {
                        if ((variant & variantOne) == 0)
                        {
                            premutation &= ~maskOne;
                        }
                        else
                        {
                            premutation |= maskOne;
                        }
                        variantOne <<= 1;
                    }
                    maskOne <<= 1;
                }
                while (variantOne < lastVariantOne);

                yield return premutation;
            }
        }

        // Метод Петрика
        public static IEnumerable<uint> ReduceImplicants(uint[] impls, List<uint> terms, int numBits)
        {
            var implCoverages = new uint[impls.Length];
            var sortedImpls = impls
                .OrderByDescending(impl => (((uint)Bit.OnesCount(impl >> numBits)) << numBits) + (impl >> numBits))
                .ToArray();
            uint coverage;

            for (int implNum = 0; implNum < sortedImpls.Length; implNum++)
            {
                uint impl = sortedImpls[implNum];
                uint termOne = 1;
                coverage = 0;

                foreach (uint term in terms)
                {
                    uint mask = ~(impl >> numBits) & Bit.Ones(numBits);
                    if ((mask & impl) == (mask & term))
                    {
                        coverage |= termOne;
                    }

                    termOne <<= 1;
                }

                implCoverages[implNum] = coverage;
            }

            uint implOnes = 0;
            uint maxOnes = Bit.MaxNum(sortedImpls.Length);
            coverage = 0;
            uint maxCoverage = Bit.MaxNum(terms.Count);

            while (implOnes < maxOnes && coverage != maxCoverage)
            {
                coverage = 0;
                implOnes++;

                for (int implNum = 0; implNum < sortedImpls.Length; implNum++)
                {
                    if ((implOnes & (1u << implNum)) != 0)
                    {
                        coverage |= implCoverages[implNum];
                    }
                }

            }

            for (int implNum = 0; implNum < sortedImpls.Length; implNum++)
            {
                if ((implOnes & (1u << implNum)) != 0)
                {
                    yield return sortedImpls[implNum];
                }
            }
        }
    }
}