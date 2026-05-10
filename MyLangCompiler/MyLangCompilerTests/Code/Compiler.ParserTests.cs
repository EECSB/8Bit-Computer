using MyLangCompiler;
using static MyLangCompiler.Compiler;

namespace MyLangCompilerTests
{
    [TestClass]
    public class ParserTests
    {
        private List<Token> TokenList(params Token[] tokens) => new List<Token>(tokens);

        [TestMethod]
        public void Peek_ReturnsCorrectToken()
        {
            var tokens = TokenList(
                new Token(Token.TokenType.Identifier, "x", 0, 0),
                new Token(Token.TokenType.Equals, "=", 0, 0),
                new Token(Token.TokenType.Number, "1", 0, 0),
                new Token(Token.TokenType.Semicolon, ";", 0, 0),
                new Token(Token.TokenType.EOF, "", 0, 0)
            );

            var parser = CreateParser(tokens);
            var peeked = InvokePeek(parser, 2);

            Assert.AreEqual(Token.TokenType.Number, peeked.Type);
            Assert.AreEqual("1", peeked.Value);
        }

        [TestMethod]
        public void NextToken_AdvancesCurrentToken()
        {
            var tokens = TokenList(
                new Token(Token.TokenType.Identifier, "x", 0, 0),
                new Token(Token.TokenType.Equals, "=", 0, 0),
                new Token(Token.TokenType.EOF, "", 0, 0)
            );

            var parser = CreateParser(tokens);
            var current = GetCurrentToken(parser);

            Assert.AreEqual(Token.TokenType.Identifier, current.Type);
            InvokeNextToken(parser);
            current = GetCurrentToken(parser);
            Assert.AreEqual(Token.TokenType.Equals, current.Type);
        }

        [TestMethod]
        public void Match_ReturnsAndAdvancesOnCorrectType()
        {
            var tokens = TokenList(
                new Token(Token.TokenType.Identifier, "x", 0, 0),
                new Token(Token.TokenType.Equals, "=", 0, 0),
                new Token(Token.TokenType.EOF, "", 0, 0)
            );

            var parser = CreateParser(tokens);
            var matched = InvokeMatch(parser, Token.TokenType.Identifier);

            Assert.AreEqual("x", matched.Value);
            var current = GetCurrentToken(parser);
            Assert.AreEqual(Token.TokenType.Equals, current.Type);
        }

        [TestMethod]
        public void Match_ThrowsOnWrongType()
        {
            var tokens = TokenList(
                new Token(Token.TokenType.Identifier, "x", 0, 0),
                new Token(Token.TokenType.Equals, "=", 0, 0),
                new Token(Token.TokenType.EOF, "", 0, 0)
            );

            var parser = CreateParser(tokens);

            Assert.ThrowsException<Exception>(() => InvokeMatch(parser, Token.TokenType.Number));
        }

        [TestMethod]
        public void MatchAny_ReturnsAndAdvancesOnAnyType()
        {
            var tokens = TokenList(
                new Token(Token.TokenType.Identifier, "x", 0, 0),
                new Token(Token.TokenType.Equals, "=", 0, 0),
                new Token(Token.TokenType.EOF, "", 0, 0)
            );

            var parser = CreateParser(tokens);
            var matched = InvokeMatchAny(parser, Token.TokenType.Number, Token.TokenType.Identifier);

            Assert.AreEqual("x", matched.Value);
            var current = GetCurrentToken(parser);
            Assert.AreEqual(Token.TokenType.Equals, current.Type);
        }

        [TestMethod]
        public void MatchAny_ThrowsOnNoMatch()
        {
            var tokens = TokenList(
                new Token(Token.TokenType.Identifier, "x", 0, 0),
                new Token(Token.TokenType.Equals, "=", 0, 0),
                new Token(Token.TokenType.EOF, "", 0, 0)
            );

            var parser = CreateParser(tokens);

            Assert.ThrowsException<Exception>(() => InvokeMatchAny(parser, Token.TokenType.Number, Token.TokenType.Plus));
        }

        //Reflection helpers to access private members

        private object CreateParser(List<Token> tokens)
        {
            var parserType = typeof(Compiler).GetNestedType("Parser", System.Reflection.BindingFlags.NonPublic);

            return Activator.CreateInstance(parserType, tokens, "");
        }

        private Token InvokePeek(object parser, int offset)
        {
            try
            {
                var method = parser.GetType().GetMethod("peek", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                return (Token)method.Invoke(parser, new object[] { offset });
            }
            catch (Exception)
            {
                throw new Exception("Error invoking match");
            }
        }

        private void InvokeNextToken(object parser)
        {
            try
            {
                var method = parser.GetType().GetMethod("nextToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(parser, null);
            }
            catch (Exception)
            {
                throw new Exception("Error invoking match");
            }
        }

        private Token InvokeMatch(object parser, Token.TokenType type)
        {
            try
            {
                var method = parser.GetType().GetMethod("match", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                return (Token)method.Invoke(parser, new object[] { type });
            }
            catch (Exception)
            {
                throw new Exception("Error invoking match");
            }
        }

        private Token InvokeMatchAny(object parser, params Token.TokenType[] types)
        {
            try
            {
                var method = parser.GetType().GetMethod("matchAny", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                return (Token)method.Invoke(parser, new object[] { types });
            }
            catch (Exception)
            {
                throw new Exception("Error invoking match");
            }
        }

        private Token GetCurrentToken(object parser)
        {
            var field = parser.GetType().GetField("current", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return (Token)field.GetValue(parser);
        }
    }
}