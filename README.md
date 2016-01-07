

We worked on a C# project and we needed to replace the body of methods of a library by `throw new NotImplementedException()` expression. We could remove the body of methods manually, but it required much time to work on it. Also when the library was updated we should do it again! We decided to do it programmatically. To do this we should parse the source code and find body of methods and replace the body by our specified expression. It is so hard :). There is many modifiers and styles to write a method in C#. Also we should remove the body of properties too.

After searching and asking a question in StackOverflow.com I found out that we can use Roslyn Library. This is description of Roslyn from MSDN:

>The .NET Compiler Platform (“Roslyn”) provides open-source C# and Visual Basic compilers with rich code analysis APIs. You can build code analysis tools with the same APIs that Microsoft is using to implement Visual Studio!


**Note:** Before using this project you should install `End User Preview` and `SDK Preview`.

For more information read [this link](http://msdn.microsoft.com/en-us/vstudio/roslyn.aspx).
