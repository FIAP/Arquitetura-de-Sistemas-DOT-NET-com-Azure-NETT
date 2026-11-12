using Domain.Entities.Base;
using Dominio.Entidades;

namespace Domain.Tests.Entidades;

public class UsuarioTests
{

    public UsuarioTests()
    {

    }


    [Fact(DisplayName = "Cadastrando um novo usuario")]
    [Trait("Cadastro", "Usuario")]
    public void User_Validate_If_Name_Is_Empty_Return_Exception()
    {

        //Arrange & Act
        var result = Assert.Throws<DomainException>(() => new Usuario("","tadriano@teste.com","10203040"));

        //Assert
        Assert.Equal("O nome não pode estar vazio!", result.Message);
    }







}
