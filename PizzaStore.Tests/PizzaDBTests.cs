using PizzaStore.DB;

namespace PizzaStore.Tests;

public class PizzaDBTests
{
    [Fact]
    public void GetPizzas_ReturnsSeededPizzas()
    {
        var pizzas = PizzaDB.GetPizzas();

        Assert.NotEmpty(pizzas);
        Assert.Contains(pizzas, pizza => pizza.Id == 1);
    }

    [Fact]
    public void GetPizza_ReturnsPizza_WhenIdExists()
    {
        var pizza = PizzaDB.GetPizza(1);

        Assert.NotNull(pizza);
        Assert.Equal(1, pizza.Id);
        Assert.Contains("Montemagno", pizza.Name);
    }
}
