
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace Roslyn_07
{
    class Program
    {
        static void Main(string[] args)
        {
            MSBuildWorkspace ws = MSBuildWorkspace.Create();

			/* Warning: This will change your source code */
			/* Set your project file here */
            Project currProject = ws.OpenProjectAsync(@"E:\Projects\Project.csproj").Result;

            foreach (var docId in currProject.DocumentIds)
            {
                Document doc = currProject.GetDocument(docId);

                Document newDoc = ProcessCode(doc);
                currProject.RemoveDocument(docId);
                MakeFileWritable(newDoc.FilePath);
                
                string code = newDoc.GetTextAsync().Result.ToString();

                File.WriteAllText(newDoc.FilePath,code);
            }
            Console.WriteLine("Finished!");
            Console.ReadKey();
        }

        static void MakeFileWritable(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            info.IsReadOnly = false;
        }



        static Document ProcessCode(Document doc)
        {
            SyntaxTree tree = doc.GetSyntaxTreeAsync().Result;
            CompilationUnitSyntax root = (CompilationUnitSyntax) tree.GetRoot();

            root = RemoveRegions(root);
            root = AddRequiredUsings(root);
            root = RemovePrivateMethods(root);
            root = RemovePrivateFields(root);
            root = DumpEvents(root);
            root = DumpMethods(root);
            root = DumpConversionOperators(root);
            root = DumpConstructors(root);
            root = DumpProperties(root);

            
            return doc.WithSyntaxRoot(root);

        }

        static CompilationUnitSyntax RemoveRegions(CompilationUnitSyntax root)
        {
            return root;
        }

        static CompilationUnitSyntax AddRequiredUsings(CompilationUnitSyntax root)
        {
            NameSyntax name = SyntaxFactory.IdentifierName(" System");
            if (root.Usings.Contains(SyntaxFactory.UsingDirective(name)) == false)
                return root.AddUsings(SyntaxFactory.UsingDirective(name));

            return root;
        }

        static CompilationUnitSyntax RemovePrivateMethods(CompilationUnitSyntax root)
        {
            List<SyntaxNode> removes = new List<SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var @class in classes)
            {
                var methods = @class.Members.OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {
                    SyntaxTokenList modifiers = method.Modifiers;
                    bool result = false;
                    foreach (SyntaxToken m in modifiers)
                    {
                        if (m.Text.Equals("private"))
                        {
                            result = true;
                            removes.Add(method);
                            break;
                        }
                    }
                        
                }
            }

            root = root.RemoveNodes(removes,SyntaxRemoveOptions.KeepDirectives);

            return root;
        }

        static CompilationUnitSyntax RemovePrivateFields(CompilationUnitSyntax root)
        {
            List<SyntaxNode> removes = new List<SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var @class in classes)
            {
                var fields = @class.Members.OfType<FieldDeclarationSyntax>();
                foreach (var field in fields)
                {
                    SyntaxTokenList modifiers = field.Modifiers;
                    bool result = false;
                    foreach (SyntaxToken m in modifiers)
                    {
                        if (m.Text.Equals("private"))
                        {
                            result = true;
                            removes.Add(field);
                            break;
                        }
                    }

                }
            }

            root = root.RemoveNodes(removes, SyntaxRemoveOptions.KeepDirectives);

            return root;
        }

        static CompilationUnitSyntax DumpMethods(CompilationUnitSyntax root)
        {
            Dictionary<SyntaxNode, SyntaxNode> changes = new Dictionary<SyntaxNode, SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var @class in classes)
            {
                var methods = @class.Members.OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {                
                    BlockSyntax newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
                    BlockSyntax body = method.Body;
                    if (body == null) continue;
                    var modifiedMethod = method.ReplaceNode(body, newBody);
                    changes.Add(method, modifiedMethod);
                }
            }

            root = root.ReplaceNodes(changes.Keys, (n1, n2) => changes[n1]);

            return root;
        }

        static CompilationUnitSyntax DumpConversionOperators(CompilationUnitSyntax root)
        {
            Dictionary<SyntaxNode, SyntaxNode> changes = new Dictionary<SyntaxNode, SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            //            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var @class in classes)
            {
                var operators = @class.Members.OfType<ConversionOperatorDeclarationSyntax>();
                foreach (var op in operators)
                {
                    BlockSyntax newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
                    BlockSyntax body = op.Body;
                    if (body == null) continue;
                    var modifiedMethod = op.ReplaceNode(body, newBody);
                    changes.Add(op, modifiedMethod);
                }
            }

            root = root.ReplaceNodes(changes.Keys, (n1, n2) => changes[n1]);

            return root;
        }

        static CompilationUnitSyntax DumpConstructors(CompilationUnitSyntax root)
        {
            Dictionary<SyntaxNode, SyntaxNode> changes = new Dictionary<SyntaxNode, SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var @class in classes)
            {
                var constructors = @class.Members.OfType<ConstructorDeclarationSyntax>();
                var destructors = @class.Members.OfType<DestructorDeclarationSyntax>();
                
                foreach (var c in constructors)
                {
                    BlockSyntax newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
                    BlockSyntax body = c.Body;
                    if (body == null) continue;
                    var modifiedMethod = c.ReplaceNode(body, newBody);
                    changes.Add(c, modifiedMethod);
                }

                foreach (var c in destructors)
                {
                    BlockSyntax newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
                    BlockSyntax body = c.Body;
                    if (body == null) continue;
                    var modifiedMethod = c.ReplaceNode(body, newBody);
                    changes.Add(c, modifiedMethod);
                }
            }

            root = root.ReplaceNodes(changes.Keys, (n1, n2) => changes[n1]);
            return root;
        }

        static CompilationUnitSyntax DumpProperties(CompilationUnitSyntax root)
        {
            Dictionary<SyntaxNode, SyntaxNode> changes = new Dictionary<SyntaxNode, SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var @class in classes)
            {
                var properties = @class.Members.OfType<PropertyDeclarationSyntax>();
                foreach (var property in properties)
                {
                    AccessorDeclarationSyntax getter, setter;
                    TryGetAccessors(property, out getter, out setter);

                    BlockSyntax newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
                    BlockSyntax getterBody = null;
                    BlockSyntax setterBody = null;
                    PropertyDeclarationSyntax modifiedProperty = null;
                    if (getter != null)
                    {
                        getterBody = getter.Body;
                        if (getterBody == null) continue;
                        changes.Add(getterBody, newBody);
                    }

                    if (setter != null)
                    {
                        setterBody = setter.Body;
                        if (setterBody == null) continue;
                        changes.Add(setterBody, newBody);
                    }
                }
                
            }

            root = root.ReplaceNodes(changes.Keys, (n1, n2) => changes[n1]);
            return root;
        }

        internal static bool TryGetAccessors(PropertyDeclarationSyntax property,out AccessorDeclarationSyntax getter,out AccessorDeclarationSyntax setter)
        {
            var accessors = property.AccessorList.Accessors;
            getter = accessors.FirstOrDefault(ad => ad.CSharpKind() == SyntaxKind.GetAccessorDeclaration);
            setter = accessors.FirstOrDefault(ad => ad.CSharpKind() == SyntaxKind.SetAccessorDeclaration);

            return accessors.Count == 2 && getter != null && setter != null;
        }


        static CompilationUnitSyntax DumpEvents(CompilationUnitSyntax root)
        {
            Dictionary<SyntaxNode, SyntaxNode> changes = new Dictionary<SyntaxNode, SyntaxNode>();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var @class in classes)
            {
                var events = @class.Members.OfType<EventDeclarationSyntax>();
                foreach (var @event in events)
                {
                    AccessorDeclarationSyntax add, remove;
                    BlockSyntax newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
                    AccessorDeclarationSyntax adder = SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration,newBody);
                    AccessorDeclarationSyntax remover = SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, newBody);
                    SyntaxList<AccessorDeclarationSyntax> syntaxes = new SyntaxList<AccessorDeclarationSyntax>();
                    syntaxes = syntaxes.Add(adder);
                    syntaxes = syntaxes.Add(remover);
                    AccessorListSyntax accessors = SyntaxFactory.AccessorList(syntaxes);
                    
                    EventDeclarationSyntax modifiedEvent = @event.WithAccessorList(accessors);
                    changes.Add(@event, modifiedEvent);
                }

            }

            root = root.ReplaceNodes(changes.Keys, (n1, n2) => changes[n1]);
            return root;
        }

        internal static bool TryGetEventAccessors(EventDeclarationSyntax @event, out AccessorDeclarationSyntax add, out AccessorDeclarationSyntax remove)
        {
            var accessors = @event.AccessorList.Accessors;
            add = accessors.FirstOrDefault(ad => ad.CSharpKind() == SyntaxKind.GetAccessorDeclaration);
            remove = accessors.FirstOrDefault(ad => ad.CSharpKind() == SyntaxKind.SetAccessorDeclaration);

            return accessors.Count == 2 && add != null && remove != null;
        }
    }
}
