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
            return RedirectToAction("MainMenu");
        }

        ViewBag.Message = "Invalid Card Number or PIN";
        return View();
    }

    public IActionResult MainMenu()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        return View();
    }

    
    public IActionResult WithdrawCash()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View("WithdrawCash");
    }

   
    [HttpPost]
    //WithdrawCash 
    [HttpPost]
    public IActionResult WithdrawCash(decimal amount)
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        decimal currentBalance = dbHelper.GetBalance(cardNumber);

        if (amount > 0 && currentBalance >= amount)
        {
            dbHelper.UpdateBalance(cardNumber, amount, false);
            dbHelper.RecordTransaction(cardNumber, "Withdraw", amount);
            ViewBag.Message = "Withdrawal successful!";
        }
        else
        {
            ViewBag.Message = "Insufficient Balance or Invalid Amount.";
        }

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View("WithdrawCash");
    }


    //CashDeposit
    [HttpPost]
    public IActionResult CashDeposit(decimal amount)
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        if (amount > 0)
        {
            dbHelper.UpdateBalance(cardNumber, amount, true);
            dbHelper.RecordTransaction(cardNumber, "Deposit", amount);
            ViewBag.Message = "Deposit successful!";
        }
        else
        {
            ViewBag.Message = "Invalid deposit amount.";
        }

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View("CashDeposit");
    }


    public IActionResult CheckBalance()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View("CheckBalance");
    }

    public IActionResult ChangePin()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        return View("ChangePin");
    }

    [HttpPost]
    public IActionResult ChangePin(string currentPin, string newPin, string confirmPin)
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        // Validate PINs
        if (string.IsNullOrEmpty(currentPin) || string.IsNullOrEmpty(newPin) || string.IsNullOrEmpty(confirmPin))
        {
            ViewBag.Message = "All fields are required.";
            return View("ChangePin");
        }

        if (newPin != confirmPin)
        {
            ViewBag.Message = "New PIN and Confirm PIN do not match.";
            return View("ChangePin");
        }

        if (!dbHelper.ValidateUser(cardNumber, currentPin))
        {
            ViewBag.Message = "Current PIN is incorrect.";
            return View("ChangePin");
        }

       
        dbHelper.UpdatePin(cardNumber, newPin);
        ViewBag.Message = "PIN successfully changed!";
        return View("ChangePin");
    }

    public IActionResult TransferFunds()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View("TransferFunds");
    }

    [HttpPost]
    //TransferFunds
    [HttpPost]
    public IActionResult TransferFunds(string recipientCardNumber, decimal amount)
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        decimal currentBalance = dbHelper.GetBalance(cardNumber);

        if (!string.IsNullOrEmpty(recipientCardNumber) && dbHelper.UserExists(recipientCardNumber) && amount > 0 && currentBalance >= amount)
        {
            dbHelper.TransferFunds(cardNumber, recipientCardNumber, amount);
            dbHelper.RecordTransaction(cardNumber, "Transfer", amount, recipientCardNumber);
            ViewBag.Message = "Transfer successful!";
        }
        else
        {
            ViewBag.Message = "Invalid details or insufficient balance.";
        }

        ViewBag.Balance = dbHelper.GetBalance(cardNumber);
        return View("TransferFunds");
    }


    public IActionResult MiniStatement()
    {
        string cardNumber = HttpContext.Session.GetString("CardNumber");
        if (string.IsNullOrEmpty(cardNumber)) return RedirectToAction("Login");

        ViewBag.Transactions = dbHelper.GetTransactions(cardNumber);
        return View("MiniStatement");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
