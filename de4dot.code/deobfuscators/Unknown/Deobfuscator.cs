﻿/*
    Copyright (C) 2011 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Text.RegularExpressions;

namespace de4dot.deobfuscators.Unknown {
	class DeobfuscatorInfo : DeobfuscatorInfoBase {
		public DeobfuscatorInfo()
			: base("un") {
		}

		internal static string ObfuscatorType {
			get { return "Unknown"; }
		}

		public override string Type {
			get { return ObfuscatorType; }
		}

		public override IDeobfuscator createDeobfuscator() {
			return new Deobfuscator(new Deobfuscator.Options {
				RenameResourcesInCode = false,
				ValidNameRegex = validNameRegex.get(),
			});
		}
	}

	class Deobfuscator : DeobfuscatorBase {
		string obfuscatorName = "Unknown Obfuscator";

		internal class Options : OptionsBase {
		}

		public override string Type {
			get { return DeobfuscatorInfo.ObfuscatorType; }
		}

		public override string Name {
			get { return obfuscatorName; }
		}

		public Deobfuscator(Options options)
			: base(options) {
		}

		public override int detect() {
			scanForObfuscator();
			return 1;
		}

		protected override void scanForObfuscatorInternal() {
			var name = scanTypes();
			if (name != null)
				obfuscatorName = name;
		}

		string scanTypes() {
			foreach (var type in module.Types) {
				if (type.FullName == "BabelAttribute")
					return "Babel .NET";
				if (type.Namespace == "___codefort")
					return "CodeFort";
				if (type.FullName == "____KILL")
					return "DeployLX CodeVeil";
				if (type.FullName == "CryptoObfuscator.ProtectedWithCryptoObfuscatorAttribute")
					return "Crypto Obfuscator";
				if (type.FullName == "AssemblyObfuscatedByGoliath" || type.FullName == "Goliath.NET.Obfuscator.Attribute.ObfuscatedByGoliath")
					return "Goliath .NET";
				if (type.FullName == "Xenocode.Client.Attributes.AssemblyAttributes.ProcessedByXenocode")
					return "Xenocode";
				if (type.FullName == "ZYXDNGuarder")
					return "DNGuard HVM";
				if (type.FullName == "InfaceMaxtoCode")
					return "MaxtoCode";
			}
			return checkDotNetReactor() ?? checkCryptoObfuscator() ?? checkMancoObfuscator();
		}

		string checkDotNetReactor() {
			foreach (var type in module.Types) {
				if (Regex.IsMatch(type.Name, @"^<PrivateImplementationDetails>\{[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}\}$")) {
					foreach (var method in type.Methods) {
						if (!method.IsStatic)
							continue;
						if (Regex.IsMatch(method.Name, @"^CS\$\d+\$[0-9A-F]{4}$"))
							return ".NET Reactor";
					}
					foreach (var field in type.Fields) {
						if (!field.IsStatic)
							continue;
						if (Regex.IsMatch(field.FieldType.Name, @"^__StaticArrayInitTypeSize=[1-9]\d*$") ||
							Regex.IsMatch(field.Name, @"^fieldimpl[1-9]\d*$") ||
							Regex.IsMatch(field.Name, @"^\$\$method0x6[0-9a-f]{6}-\d+$") ||
							Regex.IsMatch(field.Name, @"^CS\$\d+\$[0-9A-F]{4}$"))
							return ".NET Reactor";
					}
				}
			}
			return null;
		}

		string checkCryptoObfuscator() {
			int matched = 0;
			foreach (var type in module.Types) {
				if (type.Namespace != "A")
					continue;
				if (Regex.IsMatch(type.Name, "^c[0-9a-f]{32}$") || Regex.IsMatch(type.Name, "^A[A-Z]*$")) {
					if (++matched >= 20)
						return "Crypto Obfuscator";
				}
			}
			return null;
		}

		string checkMancoObfuscator() {
			int matched = 0;
			const string substr = "();\t";
			foreach (var type in module.GetTypes()) {
				if (type.Name.Contains(substr)) {
					if (++matched >= 20)
						return "Manco .NET Obfuscator";
				}
				foreach (var field in type.Fields) {
					if (field.Name.Contains(substr)) {
						if (++matched >= 20)
							return "Manco .NET Obfuscator";
					}
				}
			}
			return null;
		}
	}
}