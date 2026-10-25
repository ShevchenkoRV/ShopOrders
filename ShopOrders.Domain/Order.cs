using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopOrders.Domain;

/// <summary>
/// Замовлення покупця: склад рядків, стан і розрахунок суми до сплати.
/// </summary>
public class Order
{
    private readonly List<string[]> _lines = new List<string[]>();

    // public string prim; // примітка, поки не треба

    /// <summary>
    /// Створює нове замовлення у стані «новий».
    /// </summary>
    /// <param name="id">Номер замовлення.</param>
    /// <param name="customerName">Прізвище покупця.</param>
    public Order(string id, string customerName)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Номер замовлення.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Прізвище покупця.
    /// </summary>
    public string CustomerName { get; private set; }

    /// <summary>
    /// Поточний стан: 0 — новий, 1 — оплачений, 2 — відправлений, 3 — скасований.
    /// </summary>
    public int Status { get; private set; }

    /// <summary>
    /// Дата й час створення замовлення.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Шукає замовлення за номером у списку.
    /// </summary>
    /// <param name="orders">Список замовлень, у якому виконується пошук.</param>
    /// <param name="orderId">Шуканий номер замовлення.</param>
    /// <returns>Перше знайдене замовлення або null, якщо такого немає.</returns>
    public static Order FindById(List<Order> orders, string orderId)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].Id == orderId)
            {
                return orders[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до замовлення рядок із товаром.
    /// </summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="quantity">Кількість одиниць.</param>
    /// <param name="unitPrice">Ціна за одиницю, грн.</param>
    public void AddLine(string sku, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = sku;
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює підсумкову суму замовлення з урахуванням знижок і податку.
    /// </summary>
    /// <param name="isRegularCustomer">Ознака постійного покупця.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularCustomer)
    {
        decimal total = 0;
        int lineCount = 0;

        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total = total + quantity * unitPrice;
            lineCount = lineCount + 1;
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка

        // Знижка постійному покупцю і знижка на велике замовлення не поєднуються
        if (isRegularCustomer == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total = total + total * 0.2m;
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан замовлення, якщо перехід дозволений
    /// правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан замовлення.</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє, чи замовлення заповнене коректно.
    /// </summary>
    /// <returns>
    /// true, якщо замовлення пройшло всі перевірки; інакше false.
    /// </returns>
    public bool IsValid()
    {
        if (Id != null && Id != ""
            && CustomerName != null
            && CustomerName.Length > 2
            && _lines.Count > 0
            && _lines.Count < 100
            && Status >= 0
            && Status <= 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Формує текстовий звіт про замовлення.
    /// </summary>
    /// <returns>Текст звіту: рядки замовлення та підсумкова сума.</returns>
    public string BuildReport()
    {
        string report = "";
        for (int i = 0; i < _lines.Count; i++)
        {
            string[] line = _lines[i];
            decimal amount = int.Parse(line[1]) * decimal.Parse(line[2]);
            report = report + "Товар: " + line[0] + "; кількість: " + line[1]
                + "; ціна: " + line[2] + "; сума: " + amount + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
