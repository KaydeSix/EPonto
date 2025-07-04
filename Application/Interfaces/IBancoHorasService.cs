﻿using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBancoHorasService
    {
        Task<BancoHorasDTO> ProcessarBancoHorasDiarioAsync(int idUsuario, DateTime data);
        Task<BancoHorasDTO> ObterSaldosUsuarioAsync(int idUsuario);
        Task<BancoHorasDTO> ObterBancoHorasAtualAsync(int idUsuario);
        Task<BancoHorasDTO> ObterHorasTrabalhadasPorMesAsync(int idUsuario);
        Task<BancoHorasDTO> ObterHorasExtrasPorMesAsync(int idUsuario);
    }
}
