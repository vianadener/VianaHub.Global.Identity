using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;

namespace VianaHub.Global.Identity.Test;

public class TestAutoMapperSetup
{
    public static void TestSetup()
    {
        // Teste para descobrir a API correta do AutoMapper 16.1.1
        var profiles = new[] 
        {
            new UserMappingProfile()
        };
        
        // Tenta várias abordagens
    }
}
