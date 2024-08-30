using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoBot1.Servicos
{
    public class InvoiceService
    {
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public string Name { get; set; }
        public double Cost { get; set; }
    }

    public class InvoiceActions
    {
        private readonly List<InvoiceService> _invoices = new List<InvoiceService>();

        public void CreateInvoice(InvoiceService invoice)
        {
            _invoices.Add(invoice);
        }

        public List<InvoiceService> GetInvoices()
        {
            return _invoices;
        }

        public InvoiceService GetInvoiceByOrderId(int orderId)
        {
            return _invoices.Find(i => i.OrderId == orderId);
        }
        public List<InvoiceService> GetInvoicesByCustomer(string customerName)
        {
            return _invoices.Where(i => i.CustomerName.Equals(customerName, StringComparison.OrdinalIgnoreCase)).ToList();
        }


    }

    public class InvoiceExample
    {
        private readonly List<InvoiceService> _invoices;

        public InvoiceExample()
        {
            // Inicializar com algumas faturas de exemplo, cada uma com uma lista de produtos
            _invoices = new List<InvoiceService>
            {
                new InvoiceService
                {
                    InvoiceId = 1,
                    CustomerName = "Cliente A",
                    Amount = 150.00m,
                    InvoiceDate = DateTime.Now.AddDays(-10),
                    Products = new List<Product>
                    {
                        new Product { Name = "Produto 1", Cost = 50.00 },
                        new Product { Name = "Produto 2", Cost = 100.00 }
                    }
                },
                new InvoiceService
                {
                    InvoiceId = 2,
                    CustomerName = "Cliente B",
                    Amount = 250.00m,
                    InvoiceDate = DateTime.Now.AddDays(-5),
                    Products = new List<Product>
                    {
                        new Product { Name = "Produto 3", Cost = 150.00 },
                        new Product { Name = "Produto 4", Cost = 100.00 }
                    }
                },
                new InvoiceService
                {
                    InvoiceId = 3,
                    CustomerName = "Cliente C",
                    Amount = 100.00m,
                    InvoiceDate = DateTime.Now.AddDays(-2),
                    Products = new List<Product>
                    {
                        new Product { Name = "Produto 5", Cost = 100.00 }
                    }
                },
            };
        }


    }
}
