Tiger
-----

Para utilizar este compilador puede acceder a [MatcomOnlineGrader](http://matcomgrader.com) y resolver problemas de programación seleccionando _Tiger_.


Descripción del proyecto
------------------------

El proyecto se organizó en cuatro fases:

1. Implementación de la gramática.
2. Construcción del AST.
3. Chequeo Semántico.
4. Generación de Código.

#### 1. Implementación de la gramática.
Durante la primera fase encontramos la necesidad (particular) de utilizar varios recursos de ANTLR (v.3.4) para desambiguar la gramática. Por ejemplo:

1. Utilizamos `k = 2` en las opciones de ANTLR.
2. Durante la implementación del IF-THEN-ELSE tuvimos que utilizar la opción
`greedy=true` para hacer corresponder a cada ELSE el IF más reciente.
3. Igualmente tuvimos que utilizar la opción (`greedy=true`) para las operaciones
aritméticas, lógicas y de comparación para evitar los warnings y resolver el no-
determinismo.
4. Utilizamos el operador `=>` para resolver el conflicto entre un _lvalue_ y la creación de un _array_.

El archivo que define la gramática se encuentra en `TigerCompiler/Parsing/Tiger.g`. 


#### 2. Construcción Del AST.
El AST de nuestro compilador para Tiger está formado por 61 nodos. El nodo “más arriba” en la jerarquía es **TigerNode**, abstracto y conteniendo definiciones para el posterior chequeo semántico y generación de código. De **TigerNode** heredan **ProgramNode**, **UtilNode** y **ExpressionNode**. **ProgramNode** es el nodo que representa a todo el programa y está compuesto por un solo nodo de tipo **ExpressionNode**. “Dentro” de **UtilNode** se encuentran aquellos nodos que nos servirán de ayuda durante el proceso de chequeo semántico, algunos de estos están relacionados con tipos, declaraciones, parámetros, campos dentro de los records, etc. Un **ExpressionNode** contiene una propiedad que indica el tipo de dicho nodo (_Builint_, _AliasType_, _ArrayType_, _RecordType_). Dentro de los tipos _Builint_ se encuentran los enteros (_int_), cadenas (_string_), _void_, _nil_, _error_. Entonces hay expresiones que devuelven valor y otras que no, por ejemplo, las expresiones **for**, **while**, **if-then**, **break**, etc... siempre tendrán tipo void. El tipo _nil_ se dará solamente al nodo compuesto por el token _nil_ (**NilNode**). [Ver código para más información...]


#### 3. Chequeo Semántico.
Todo nodo en el AST implementa el siguiente método:

`void CheckSemantic(Scope scope, List<SemanticError> errors)`

La clase **Scope** contiene las herramientas necesarias para localizar las variables, funciones y tipos correspondientes durante el chequeo semántico. La lista _errors_ contendrá todos los errores que se detecten durante este proceso. Durante esta etapa toda variable quedará asociada a un **VariableInfo**, toda función a un **FunctionInfo** y todo tipo a un **TypeInfo**. Se declarará solamente un Info para cada variable, función y tipo (no para los alias). La parte más interesante del chequeo semántico fue el la declaración de tipos y funciones en un mismo bloque. En el caso de las funciones hicimos una primera pasada registrándolas y después otra pasada haciendo _CheckSemantics_ al cuerpo de las mismas. Para el caso de los tipos, _CheckSemantics_ detecta ciclos durante la declaración de un bloque. Antes de comenzar analizando **ProgramNode** (nuestro nodo raíz en el árbol concreto para cualquier programa) registramos los tipos _int_, _string_ y todas las funciones estándares de Tiger.


#### 4. Generación de código.
Una vez realizado _CheckSemantics_ y en caso de no haber encontrado errores, seguimos con la generación de código. Todo nodo del AST implementará el siguiente método:

`void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)`

- _ModuleBuilder_: Se utiliza para registrar los tipos del programa. Solamente se registrarán los records como clases.
- _TypeBuilder_: Es donde iremos poniendo las variables, las funciones y las funciones estándares.
- _ILGenerator_: Es el cuerpo de la función actual. Al comienzo generador será el _Main_ de Program.

Todas las variables son estáticas y corresponden tanto a parámetros, variables en los **for** así como variables declaradas directamente en un **let-in-end**. Toda función será estática y no recibirá parámetro ninguno, solamente conservarán el tipo de retorno del programa en tiger (.tig). Por cada tipo definido, incluyendo _int_ y _string_, se declararán dos pilas genéricas en el tipo para ayudar al proceso de simulación de un llamado de función. Antes de llamar a una función, se insertan los valores de las variables locales a dicha función en las pilas correspondientes y después del llamado se restauran. La segunda pila es utilizada solamente para “ayudar” en el proceso del movimiento entre la primera pila y la pila de IL. Con esto, hacemos que nuestro programa no guarde la información relacionada a los llamados de funciones (_ActiveRecord_) en la pila de IL, porque esto puede provocar _Stack Overflow_ considerablemente más rápido. Por esto decidimos guardar dicha información en el Heap.

Considere el siguiente ejemplo:

```
let
	var str := "Hello World"
in
	printline(str);
end
```

Este código se traducirá en algo como:

```C#
using System;
using System.Collections.Generic;
internal class Program
{
	static Stack<int> STACK_1;
	static Stack<int> STACK_2;
	static Stack<string> STACK_3;
	static Stack<string> STACK_4;
	static string VAR_5;
	static void Main()
	{
		try
		{
			Program.run();
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(“Some Error...”);
			Environment.Exit(1);
		}
	}
	//
	// STANDARD_FUNCTIONS
	//
	public static void run()
	{
		Program.VAR_5 = "Hello World";
		Program.printline(Program.VAR_5);
	}
	static Program()
	{
		Program.STACK_1 = new Stack<int>();
		Program.STACK_2 = new Stack<int>();
		Program.STACK_3 = new Stack<string>();
		Program.STACK_4 = new Stack<string>();
	}
}
```
