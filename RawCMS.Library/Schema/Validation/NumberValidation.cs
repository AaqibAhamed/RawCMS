﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************

namespace RawCMS.Library.Schema.Validation
{
    public class NumberValidation : BaseJavascriptValidator
    {
        public override string Type => "number";

        public override string Javascript
        {
            get
            {
                return @"
if(value!=null)
{
//code starts here
intVal=parseFloat(value);
 if(isNaN(intVal) || intVal  === NaN )
 {
   errors.push({""Code"":""FLOAT - 01"", ""Title"":""Not a number""});
 }

if(options.min !==undefined)
 {
   if(options.min>intVal)
     {
       errors.push({""Code"":""FLOAT-02"", ""Title"":""less than minimum""});
     }
 }

if(options.max !==undefined)
 {
   if(options.max<intVal)
     {
      errors.push({""Code"":""FLOAT-03"", ""Title"":""greater than max""});
     }
 }
}
var backendResult=JSON.stringify(errors);
            ";
            }
        }
    }
}