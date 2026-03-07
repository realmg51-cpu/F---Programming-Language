using System;
using System.Collections.Generic;
using System.Linq;

namespace Fminusminus.Optimizer
{
    /// <summary>
    /// AST Optimizer for F--
    /// Performs various optimization passes on the Abstract Syntax Tree
    /// </summary>
    public class AstOptimizer
    {
        private readonly CodeGenerator.OptimizationLevel _level;
        private int _optimizationsApplied;

        public AstOptimizer(CodeGenerator.OptimizationLevel level)
        {
            _level = level;
        }

        public ProgramNode Optimize(ProgramNode ast)
        {
            _optimizationsApplied = 0;
            Console.WriteLine($"🔧 Applying optimizations (level {_level})...");

            var optimized = ast;

            // Basic optimizations always applied
            optimized = RemoveDeadCode(optimized);
            optimized = ConstantFolding(optimized);

            if (_level >= CodeGenerator.OptimizationLevel.O1)
            {
                optimized = ConstantPropagation(optimized);
                optimized = RemoveUnusedVariables(optimized);
            }

            if (_level >= CodeGenerator.OptimizationLevel.O2)
            {
                optimized = InlineFunctions(optimized);
                optimized = LoopOptimizations(optimized);
            }

            if (_level >= CodeGenerator.OptimizationLevel.O3)
            {
                optimized = AggressiveOptimizations(optimized);
            }

            Console.WriteLine($"✅ Applied {_optimizationsApplied} optimizations");
            return optimized;
        }

        #region Basic Optimizations (Always applied)

        /// <summary>
        /// Remove code that will never be executed
        /// </summary>
        private ProgramNode RemoveDeadCode(ProgramNode program)
        {
            if (program.StartBlock == null) return program;

            var newStatements = new List<StatementNode>();
            bool afterReturn = false;

            foreach (var stmt in program.StartBlock.Statements)
            {
                if (afterReturn)
                {
                    // This code is dead - remove it
                    _optimizationsApplied++;
                    Console.WriteLine($"  ✂️ Removed dead code after return");
                    continue;
                }

                if (stmt is ReturnStatementNode)
                {
                    afterReturn = true;
                }

                newStatements.Add(stmt);
            }

            program.StartBlock.Statements = newStatements;
            return program;
        }

        /// <summary>
        /// Evaluate constant expressions at compile time
        /// </summary>
        private ProgramNode ConstantFolding(ProgramNode program)
        {
            // This is a simplified version - would need to traverse all expressions
            // For now, just handle basic cases
            return program;
        }

        #endregion

        #region Level O1 Optimizations

        /// <summary>
        /// Propagate constant values through variables
        /// </summary>
        private ProgramNode ConstantPropagation(ProgramNode program)
        {
            if (program.StartBlock == null) return program;

            var constants = new Dictionary<string, object>();

            foreach (var stmt in program.StartBlock.Statements)
            {
                if (stmt is AssignmentNode assign && assign.Value is NumberLiteralNode num)
                {
                    constants[assign.VariableName] = num.Value;
                }
                else if (stmt is AssignmentNode assign2 && assign2.Value is StringLiteralNode str)
                {
                    constants[assign2.VariableName] = str.Value;
                }
                // Would need to replace variable uses with constants
            }

            return program;
        }

        /// <summary>
        /// Remove variables that are never used
        /// </summary>
        private ProgramNode RemoveUnusedVariables(ProgramNode program)
        {
            if (program.StartBlock == null) return program;

            var usedVars = new HashSet<string>();
            var definedVars = new HashSet<string>();

            // First pass: find all variable uses and definitions
            foreach (var stmt in program.StartBlock.Statements)
            {
                if (stmt is AssignmentNode assign)
                {
                    definedVars.Add(assign.VariableName);
                }
                else if (stmt is PrintlnStatementNode println && println.Expression is VariableNode var)
                {
                    usedVars.Add(var.Name);
                }
                else if (stmt is PrintStatementNode print && print.Expression is VariableNode var2)
                {
                    usedVars.Add(var2.Name);
                }
            }

            // Second pass: remove unused assignments
            var newStatements = new List<StatementNode>();
            foreach (var stmt in program.StartBlock.Statements)
            {
                if (stmt is AssignmentNode assign && !usedVars.Contains(assign.VariableName))
                {
                    _optimizationsApplied++;
                    Console.WriteLine($"  ✂️ Removed unused variable: {assign.VariableName}");
                    continue; // Skip this assignment
                }
                newStatements.Add(stmt);
            }

            program.StartBlock.Statements = newStatements;
            return program;
        }

        #endregion

        #region Level O2 Optimizations

        /// <summary>
        /// Inline small functions
        /// </summary>
        private ProgramNode InlineFunctions(ProgramNode program)
        {
            // Would need function definitions first
            return program;
        }

        /// <summary>
        /// Loop optimizations (invariant code motion, etc.)
        /// </summary>
        private ProgramNode LoopOptimizations(ProgramNode program)
        {
            // Would need loops first
            return program;
        }

        #endregion

        #region Level O3 Optimizations

        /// <summary>
        /// Aggressive optimizations
        /// </summary>
        private ProgramNode AggressiveOptimizations(ProgramNode program)
        {
            // Advanced optimizations:
            // - Loop unrolling
            // - Vectorization
            // - Tail recursion elimination
            // - etc.
            return program;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Print optimization statistics
        /// </summary>
        public void PrintStats()
        {
            Console.WriteLine($"📊 Optimizations applied: {_optimizationsApplied}");
        }

        #endregion
    }
}
