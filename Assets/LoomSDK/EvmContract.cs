﻿using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Loom.Nethereum.Contracts;
using Loom.Nethereum.RPC.Eth.DTOs;

namespace Loom.Unity3d
{
    /// <summary>
    /// The Contract class streamlines interaction with a smart contract that was deployed on a EVM-based Loom DAppChain.
    /// Each instance of this class is bound to a specific smart contract, and provides a simple way of calling
    /// into and querying that contract.
    /// </summary>
    public class EvmContract : Contract
    {
        private readonly ContractBuilder contractBuilder;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">Client to use to communicate with the contract.</param>
        /// <param name="contractAddr">Address of a contract on the Loom DAppChain.</param>
        /// <param name="callerAddr">Address of the caller, generated from the public key of the tx signer.</param>
        /// <param name="abi">Contract Application Binary Interface as JSON object string.</param>
        public EvmContract(DAppChainClient client, Address contractAddr, Address callerAddr, string abi) : base(client, contractAddr, callerAddr) {
            this.contractBuilder = new ContractBuilder(abi, contractAddr.LocalAddressHexString);
        }

        #region CallAsync methods

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Arguments objects arrays for the smart contract method.</param>
        /// <returns>Nothing.</returns>
        public async Task CallAsync(string method, params object[] functionInput)
        {
            FunctionBuilder function = contractBuilder.GetFunctionBuilder(method);
            CallInput callInput = function.CreateCallInput(functionInput);
            await CallAsync(callInput.Data);
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <returns>Nothing.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task CallAsync<TInput>(TInput functionInput)
        {
            FunctionBuilder<TInput> function = contractBuilder.GetFunctionBuilder<TInput>();
            CallInput callInput = function.CreateCallInput(functionInput);
            await CallAsync(callInput.Data);
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Arguments objects arrays for the smart contract method.</param>
        /// <returns>The return value of the smart contract method.</returns>
        public async Task<T> CallSimpleTypeOutputAsync<T>(string method, params object[] functionInput)
        {
            FunctionBuilder functionBuilder;
            return await CallAsync(CreateContractMethodCallInput(method, functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeSimpleTypeOutput<T>(s));
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Argument objects arrays for the smart contract method.</param>
        /// <returns>Return Data Transfer Object of the smart contract method.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> CallDTOTypeOutputAsync<TReturn>(string method, params object[] functionInput) where TReturn : new()
        {
            FunctionBuilder functionBuilder;
            return await CallAsync(CreateContractMethodCallInput(method, functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput<TReturn>(s));
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Argument objects arrays for the smart contract method.</param>
        /// <param name="functionOutput">Return Data Transfer Object of the smart contract method. A pre-existing object can be reused.</param>
        /// <returns>Return Data Transfer Object of the smart contract method. Same object instance as <paramref name="functionOutput"/>.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> CallDTOTypeOutputAsync<TReturn>(TReturn functionOutput, string method, params object[] functionInput) where TReturn : new()
        {
            FunctionBuilder functionBuilder;
            return await CallAsync(CreateContractMethodCallInput(method, functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput(functionOutput, s));
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <returns>The return value of the smart contract method.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> CallSimpleTypeOutputAsync<TInput, TReturn>(TInput functionInput)
        {
            FunctionBuilder<TInput> functionBuilder;
            return await CallAsync(CreateContractMethodCallInput(functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeSimpleTypeOutput<TReturn>(s));
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <returns>Return Data Transfer Object of the smart contract method.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> CallDTOTypeOutputAsync<TInput, TReturn>(TInput functionInput) where TReturn : new()
        {
            FunctionBuilder<TInput> functionBuilder;
            return await CallAsync(CreateContractMethodCallInput(functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput<TReturn>(s));
        }

        /// <summary>
        /// Calls a smart contract method that mutates state.
        /// The call into the smart contract is accomplished by committing a transaction to the DAppChain.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <param name="functionOutput">Return Data Transfer Object of the smart contract method. A pre-existing object can be reused.</param>
        /// <returns>Return Data Transfer Object of the smart contract method. Same object instance as <paramref name="functionOutput"/>.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> CallDTOTypeOutputAsync<TInput, TReturn>(TInput functionInput, TReturn functionOutput) where TReturn : new()
        {
            FunctionBuilder<TInput> functionBuilder;
            return await CallAsync(CreateContractMethodCallInput(functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput(functionOutput, s));
        }

        #endregion

        #region StaticCallAsync methods

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Arguments objects arrays for the smart contract method.</param>
        /// <returns>Nothing.</returns>
        public async Task StaticCallAsync(string method, params object[] functionInput)
        {
            FunctionBuilder function = contractBuilder.GetFunctionBuilder(method);
            CallInput callInput = function.CreateCallInput(functionInput);
            await StaticCallAsync(callInput.Data);
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <returns>Nothing.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task StaticCallAsync<TInput>(TInput functionInput)
        {
            FunctionBuilder<TInput> function = contractBuilder.GetFunctionBuilder<TInput>();
            CallInput callInput = function.CreateCallInput(functionInput);
            await StaticCallAsync(callInput.Data);
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Arguments objects arrays for the smart contract method.</param>
        /// <returns>The return value of the smart contract method.</returns>
        public async Task<T> StaticCallSimpleTypeOutputAsync<T>(string method, params object[] functionInput)
        {
            FunctionBuilder functionBuilder;
            return await StaticCallAsync(CreateContractMethodCallInput(method, functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeSimpleTypeOutput<T>(s));
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Argument objects arrays for the smart contract method.</param>
        /// <returns>Return Data Transfer Object of the smart contract method.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> StaticCallDTOTypeOutputAsync<TReturn>(string method, params object[] functionInput) where TReturn : new()
        {
            FunctionBuilder functionBuilder;
            return await StaticCallAsync(CreateContractMethodCallInput(method, functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput<TReturn>(s));
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="method">Smart contract method name.</param>
        /// <param name="functionInput">Argument objects arrays for the smart contract method.</param>
        /// <param name="functionOutput">Return Data Transfer Object of the smart contract method. A pre-existing object can be reused.</param>
        /// <returns>Return Data Transfer Object of the smart contract method. Same object instance as <paramref name="functionOutput"/>.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> StaticCallDTOTypeOutputAsync<TReturn>(TReturn functionOutput, string method, params object[] functionInput) where TReturn : new()
        {
            FunctionBuilder functionBuilder;
            return await StaticCallAsync(CreateContractMethodCallInput(method, functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput(functionOutput, s));
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <returns>The return value of the smart contract method.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> StaticCallSimpleTypeOutputAsync<TInput, TReturn>(TInput functionInput)
        {
            FunctionBuilder<TInput> functionBuilder;
            return await StaticCallAsync(CreateContractMethodCallInput(functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeSimpleTypeOutput<TReturn>(s));
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <returns>Return Data Transfer Object of the smart contract method.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> StaticCallDTOTypeOutputAsync<TInput, TReturn>(TInput functionInput) where TReturn : new()
        {
            FunctionBuilder<TInput> functionBuilder;
            return await StaticCallAsync(CreateContractMethodCallInput(functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput<TReturn>(s));
        }

        /// <summary>
        /// Calls a contract method that doesn't mutate state.
        /// This method is usually used to query the current smart contract state, it doesn't commit any transactions.
        /// </summary>
        /// <param name="functionInput">Input Data Transfer Object for smart contract method.</param>
        /// <param name="functionOutput">Return Data Transfer Object of the smart contract method. A pre-existing object can be reused.</param>
        /// <returns>Return Data Transfer Object of the smart contract method. Same object instance as <paramref name="functionOutput"/>.</returns>
        /// <see href="https://nethereum.readthedocs.io/en/latest/contracts/functiondtos/"/>
        public async Task<TReturn> StaticCallDTOTypeOutputAsync<TInput, TReturn>(TInput functionInput, TReturn functionOutput) where TReturn : new()
        {
            FunctionBuilder<TInput> functionBuilder;
            return await StaticCallAsync(CreateContractMethodCallInput(functionInput, out functionBuilder), functionBuilder, (fb, s) => fb.DecodeDTOTypeOutput(functionOutput, s));
        }

        #endregion

        protected override void NotifyContractEvent(object sender, DAppChainClient.ChainEventArgs e)
        {
            if (e.ContractAddress.Equals(this.Address))
            {
                Event evt = new Event();
                evt.MergeFrom(e.Data);
                e.Data = evt.Data.ToByteArray();

                InvokeChainEvent(sender, e);
            }
        }

        #region Call helper methods

        private async Task<byte[]> StaticCallAsyncByteArray(string callInput)
        {
            return await this.client.QueryAsync<byte[]>(this.Address, CryptoUtils.HexStringToBytes(callInput), this.Caller, VMType.Evm);
        }

        private async Task StaticCallAsync(string callInput)
        {
            await StaticCallAsyncByteArray(callInput);
        }

        private async Task<TReturn> StaticCallAsync<TReturn>(string callInput, FunctionBuilderBase functionBuilder, Func<FunctionBuilderBase, string, TReturn> decodeFunc)
        {
            var result = await StaticCallAsyncByteArray(callInput);

            var validResult = result != null && result.Length != 0;
            return validResult ? decodeFunc(functionBuilder, CryptoUtils.BytesToHexString(result)) : default(TReturn);
        }

        private async Task<BroadcastTxResult> CallAsyncBrodcastTxResult(string callInput)
        {
            var tx = this.CreateContractMethodCallTx(callInput, VMType.Evm);
            return await this.client.CommitTxAsync(tx);
        }

        private async Task CallAsync(string callInput)
        {
            await CallAsyncBrodcastTxResult(callInput);
        }

        private async Task<TReturn> CallAsync<TReturn>(string callInput, FunctionBuilderBase functionBuilder, Func<FunctionBuilderBase, string, TReturn> decodeFunc)
        {
            var tx = this.CreateContractMethodCallTx(callInput, VMType.Evm);
            var result = await this.client.CommitTxAsync(tx);
            var validResult = result?.DeliverTx.Data != null && result.DeliverTx.Data.Length != 0;
            return validResult ? decodeFunc(functionBuilder, CryptoUtils.BytesToHexString(result.DeliverTx.Data)) : default(TReturn);
        }

        private string CreateContractMethodCallInput(string method, object[] functionInput, out FunctionBuilder functionBuilder)
        {
            functionBuilder = contractBuilder.GetFunctionBuilder(method);
            CallInput callInput = functionBuilder.CreateCallInput(functionInput);
            return callInput.Data;
        }

        private string CreateContractMethodCallInput<TInput>(TInput functionInput, out FunctionBuilder<TInput> functionBuilder)
        {
            functionBuilder = contractBuilder.GetFunctionBuilder<TInput>();
            CallInput callInput = functionBuilder.CreateCallInput(functionInput);
            return callInput.Data;
        }

        private Transaction CreateContractMethodCallTx(string method, object[] functionInput, out FunctionBuilder functionBuilder)
        {
            functionBuilder = contractBuilder.GetFunctionBuilder(method);
            CallInput callInput = functionBuilder.CreateCallInput(functionInput);
            return CreateContractMethodCallTx(callInput.Data, VMType.Evm);
        }

        private Transaction CreateContractMethodCallTx<TInput>(TInput functionInput, out FunctionBuilder<TInput> functionBuilder)
        {
            functionBuilder = contractBuilder.GetFunctionBuilder<TInput>();
            CallInput callInput = functionBuilder.CreateCallInput(functionInput);
            return CreateContractMethodCallTx(callInput.Data, VMType.Evm);
        }

        #endregion
    }
}
