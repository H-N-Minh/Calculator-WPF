

public abstract class PizzaStore
{
    abstract public Pizza createPizza(string type);
    public Pizza orderPizza(String type)
    {
        Pizza pizza;
        pizza = createPizza(type);
        pizza.prepare();
        pizza.bake();
        pizza.cut();
        pizza.box();
        return pizza;
    }ca
}

public class NYStylePizzaStore : PizzaStore
{
    public override Pizza createPizza(string type)
    {
        if (type.Equals("cheese"))
        {
            pizza = new NYStyleCheesePizza();
        }
        else if (type.Equals("pepperoni")) {
            pizza = new NYStylePepperoniPizza();
        }
        else if (type.Equals("clam")) {
            pizza = new NYStyleClamPizza();
        }
        else if (type.Equals("veggie")) {
            pizza = new NYStyleVeggiePizza();
        }
    }
}

public class Pizza
{
    
}