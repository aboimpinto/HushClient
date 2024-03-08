using System.Text.Json;
using HushEcosystem.Model;
using HushEcosystem.Model.Builders;

namespace HushClient.Workflows;

public abstract class WorkflowBase
{
    public readonly TransactionBaseConverter TransactionBaseConverter;
    public readonly JsonSerializerOptions HashTransactionJsonOptions;
    public readonly JsonSerializerOptions SignTransactionJsonOptions;
    public readonly JsonSerializerOptions SendTransactionJsonOptions;

    public WorkflowBase(TransactionBaseConverter transactionBaseConverter)
    {
        this.TransactionBaseConverter = transactionBaseConverter;

        this.HashTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this.TransactionBaseConverter)
            .WithModifierExcludeSignature()
            .WithModifierExcludeBlockIndex()
            .WithModifierExcludeHash()
            .Build();

        this.SignTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this.TransactionBaseConverter)
            .WithModifierExcludeSignature()
            .WithModifierExcludeBlockIndex()
            .Build();

        this.SendTransactionJsonOptions = new JsonSerializerOptionsBuilder()
            .WithTransactionBaseConverter(this.TransactionBaseConverter)
            .WithModifierExcludeBlockIndex()
            .Build();
    }
}

