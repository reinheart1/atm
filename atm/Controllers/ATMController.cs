using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class ATMController : Controller
{
    private DatabaseHelper dbHelper = new DatabaseHelper();

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string cardNumber, string pin)
    {
        if (dbHelper.ValidateUser(cardNumber, pin))
        {
            HttpContext.Session.SetString("CardNumber", cardNumber);
            return RedirectToAction("Dashboard");
        }

        ViewBag.Message = "Invalid Card Number or PIN";
        return View();
    }

    public IActionResult Dashboard()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (cardNumber == null) return RedirectToAction("Login");

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View();
    }

    [HttpPost]
    public IActionResult Deposit(decimal amount)
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (cardNumber == null) return RedirectToAction("Login");

        dbHelper.UpdateBalance(cardNumber, amount, true);
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public IActionResult Withdraw(decimal amount)
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (cardNumber == null) return RedirectToAction("Login");

        decimal currentBalance = dbHelper.GetBalance(cardNumber);
        if (currentBalance >= amount)
        {
            dbHelper.UpdateBalance(cardNumber, amount, false);
        }
        else
        {
            ViewBag.Message = "Insufficient Balance";
        }

        return RedirectToAction("Dashboard");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
