using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace lab1
{
    internal class Calculator
    {
        public Dictionary<string, int> CalculateC(Buyer[] buyers, int combinationLength)
        {
            Dictionary<string, int> combinationsCounts = new Dictionary<string, int>();

            foreach (Buyer buyer in buyers)
            {
                Product[] buyerProducts = buyer.GetProductsCopy();

                List<Product[]> combinations = GetCombinations(buyerProducts, combinationLength);

                foreach (Product[] combo in combinations)
                {
                    string key = MakeKey(combo);

                    if (combinationsCounts.ContainsKey(key))
                        combinationsCounts[key]++;
                    else
                        combinationsCounts[key] = 1;
                }
            }

            return combinationsCounts;
        }


        public Dictionary<string, int> CalculateL(Dictionary<string, int> Ck, int minCount)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (var pair in Ck)
            {
                if (pair.Value >= minCount)
                    result[pair.Key] = pair.Value;
            }

            return result;
        }

        private List<Product[]> GetCombinations(Product[] products, int length)
        {
            List<Product[]> result = new List<Product[]>();

            Combine(products, new Product[length], 0, 0, result);

            return result;
        }

        private void Combine(Product[] products, Product[] current, int start, int index, List<Product[]> result)
        {
            if (index == current.Length)
            {
                result.Add((Product[])current.Clone());
                return;
            }

            for (int i = start; i < products.Length; i++)
            {
                current[index] = products[i];

                Combine(products, current, i + 1, index + 1, result);
            }
        }

        private string MakeKey(Product[] products)
        {
            return string.Join(" и ", products.Select(p => p.Name).OrderBy(name => name));
        }

        public Dictionary<string, int> FindFrequentItemsets(Buyer[] buyers, double minSupport, int maxLength) //найти частые наборы товаров
        {
            int minCount = (int)Math.Ceiling(buyers.Length * minSupport);

            Dictionary<string, int> allFrequent = new Dictionary<string, int>(); //все товары и их кол-во 

            for (int length = 1; length <= maxLength; length++) // подсчет комбинаций 
            {
                Dictionary<string, int> C = CalculateC(buyers, length);

                Dictionary<string, int> L = CalculateL(C, minCount); //отсеивание комбинаций 

                foreach (var pair in L)
                {
                    allFrequent[pair.Key] = pair.Value;
                }
            }

            return allFrequent;
        }

        public List<AssociationRule> FindAssociationRules(Buyer[] buyers, double minSupport, double minConfidence, int maxLength)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            int transactionCount = buyers.Length;

            Dictionary<string, int> frequentItemsets = FindFrequentItemsets(buyers, minSupport, maxLength);

            List<AssociationRule> rules = new List<AssociationRule>(); // молоко -> творог, молоко + творого ->.....    

            foreach (var pair in frequentItemsets) // создание всех комбинаций: молоко и творог и сметана 
                                                   //молоко + творог -> сметана, молоко+сметана -> творог, молоко -> смената + творог.......
            {
                string itemset = pair.Key;

                string[] items = itemset.Split(new[] { " и " }, StringSplitOptions.RemoveEmptyEntries);

                // Правило нельзя построить из одного объекта
                if (items.Length < 2)
                    continue;

                List<string[]> subsets = GetNonEmptyProperSubsets(items);

                foreach (string[] antecedentItems in subsets) // рассмотр на антецедент и консеквент
                {
                    string[] consequentItems = items.Except(antecedentItems).ToArray();

                    string antecedent = string.Join(" и ", antecedentItems.OrderBy(x => x));

                    string consequent = string.Join(" и ", consequentItems.OrderBy(x => x));

                    int unionCount = pair.Value; //количество покупок всего набора: молоко и творог и смената у 3 покупателей например 

                    string antecedentKey = string.Join(" и ", antecedentItems.OrderBy(x => x));

                    if (frequentItemsets.ContainsKey(antecedentKey) == false)
                        continue;

                    int antecedentCount = frequentItemsets[antecedentKey];

                    string consequentKey = string.Join(" и ", consequentItems.OrderBy(x => x));

                    if (frequentItemsets.ContainsKey(consequentKey) == false)
                        continue;

                    int consequentCount = frequentItemsets[consequentKey];

                    double support = (double)unionCount / transactionCount;
                    //Support(A -> B) = количество транзакций с A и B / общее количество транзакций

                    double confidence = (double)unionCount / antecedentCount;
                    //Confidence(A -> B) = количество транзакций с A и B / количество транзакций с A

                    double consequentSupport = (double)consequentCount / transactionCount;
                    //Support(B) = количество транзакций с B / общее количество транзакций

                    double lift = confidence / consequentSupport;
                    //Lift(A -> B) = Confidence(A -> B) / Support(B)

                    if (confidence >= minConfidence)
                    {
                        rules.Add(new AssociationRule(antecedent, consequent, support, confidence, lift));
                    }
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"Поиск правил завершен за {stopwatch.Elapsed.TotalMilliseconds:F2} мс");

            return RemoveDuplicates(rules);
        }

        private List<string[]> GetNonEmptyProperSubsets(string[] items)
        {
            List<string[]> result = new List<string[]>();

            int subsetCount = (1 << items.Length) - 1;

            for (int mask = 1; mask < subsetCount; mask++)
            {
                List<string> subset = new List<string>();

                for (int i = 0; i < items.Length; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        subset.Add(items[i]);
                    }
                }

                result.Add(subset.ToArray());
            }

            return result;
        }

        // Удаление одинаковых правил
        private List<AssociationRule> RemoveDuplicates(List<AssociationRule> rules)
        {
            return rules
                .GroupBy(rule => rule.Antecedent + "->" + rule.Consequent)
                .Select(group => group.First())
                .ToList();
        }

        // Сортировка правил
        public List<AssociationRule> SortRules(List<AssociationRule> rules, bool bySupportDescending)
        {
            if (bySupportDescending)
            {
                return rules
                    .OrderByDescending(r => r.Support)
                    .ThenBy(r => r.Antecedent)
                    .ThenBy(r => r.Consequent)
                    .ToList();
            }

            return rules
                .OrderBy(r => r.Antecedent)
                .ThenBy(r => r.Consequent)
                .ToList();
        }
    }
}