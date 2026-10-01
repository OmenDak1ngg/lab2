using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string filePath = @"E:\ucheba\intelanaldan\lab2\lab1\basket.csv";

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            List<Buyer> buyerList = new List<Buyer>();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] productNames = line.Split(',');

                List<Product> products = new List<Product>();

                foreach (string productName in productNames)
                {
                    Product product = new Product(productName.Trim());

                    products.Add(product);
                }

                Buyer buyer = new Buyer(products.ToArray());

                buyerList.Add(buyer);
            }

            Buyer[] buyers = buyerList.ToArray();
            Console.WriteLine("Количество покупателей: " + buyers.Length);
            Calculator calculator = new Calculator();

            double minSupport = 0.05;
            double minConfidence = 0.15;

            List<AssociationRule> rules =
                calculator.FindAssociationRules(
                    buyers,
                    minSupport,
                    minConfidence,
                    7);
            Console.WriteLine("Найдено правил до сортировки: " + rules.Count);
            rules = calculator.SortRules(rules, true);

            Console.WriteLine();
            Console.WriteLine("НАЙДЕННЫЕ АССОЦИАТИВНЫЕ ПРАВИЛА");
            Console.WriteLine(new string('-', 70));

            foreach (AssociationRule rule in rules)
            {
                Console.WriteLine(rule);
            }

            Console.WriteLine();
            Console.WriteLine("Всего правил: " + rules.Count);

            Console.ReadKey();
        }
    }
}