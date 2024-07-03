public class ApiResponseUserTransactions : ApiResponseUser
{
    public List<PromiseTransaction>? Transactions { get; set; }
}
