﻿using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class BancoHorasDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public IEnumerable<SaldoDiarioBancoHorasModel> SaldosDiarios { get; set; }
        public IEnumerable<BancoHorasModel> BancoHoras { get; set; }
    }
}
