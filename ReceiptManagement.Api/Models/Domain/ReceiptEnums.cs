namespace ReceiptManagement.Api.Models.Domain;

public enum ReceiptPaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    EWallet,
    BankTransfer
}

public enum ReceiptStatus
{
    Draft,
    Recorded,
    Reimbursed,
    Archived
}
