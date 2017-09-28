﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentProcessor.BetterChainOfResponsibility
{
    public class EftPaymentTypeHandler : PaymentTypeHandlerBase
    {


        public EftPaymentTypeHandler(IPaymentTypeHandler nextPaymentHandler, IEftProcessor eftProcessor,
            IPaymentsDao paymentsDao) 
            : base(nextPaymentHandler)
        {
            this.eftProcessor = eftProcessor;
            this.paymentsDao = paymentsDao;
        }


        private IEftProcessor eftProcessor;
        private IPaymentsDao paymentsDao;



        protected override bool CanProcessPayment(PaymentDataBase paymentData)
        {
            return paymentData.PaymentType == PaymentType.EFT;
        }

        protected override PaymentResult ExecutePaymentProcess(PaymentDataBase paymentData)
        {
            EftPaymentData eftPaymentData = paymentData as EftPaymentData;

            EftAuthorization eftResult = this.eftProcessor.AuthorizeEftPayment(eftPaymentData.RoutingNumber,
                eftPaymentData.BankAccountNumber, eftPaymentData.AccountType, eftPaymentData.Amount);

            if (eftResult.Authorized)
            {
                int referenceNumber = paymentsDao.SaveSuccessfulEftPayment(eftPaymentData, eftResult);

                PaymentResult paymentResult = new PaymentResult()
                {
                    CustomerAccountNumber = eftPaymentData.CustomerAccountNumber,
                    PaymentDate = eftPaymentData.PaymentDate,
                    Success = eftResult.Authorized,
                    ReferenceNumber = referenceNumber
                };

                return paymentResult;
            }
            else
            {
                int referenceNumber = paymentsDao.SaveFailedEftPayment(eftPaymentData, eftResult);

                PaymentResult paymentResult = new PaymentResult()
                {
                    CustomerAccountNumber = eftPaymentData.CustomerAccountNumber,
                    PaymentDate = eftPaymentData.PaymentDate,
                    Success = eftResult.Authorized,
                    ReferenceNumber = referenceNumber
                };
                return paymentResult;
            }
        }
    }
}
