# ArmDotPatcher

**ArmDotPatcher** is a small utility that patches `ArmDot.Engine.dll`, the core runtime of the [ArmDot obfuscator](https://www.armdot.com/), to remove the default 7-day obfuscation trial limit.

It searches for methods in the `ArmDot.Engine.CodeConverters.HighLevel` namespace that call `DateTime.UtcNow` and replaces them with a hardcoded `DateTime(3000, 1, 1)` — effectively bypassing any expiration checks built into the obfuscated assembly.

This patcher is useful for reverse engineers and security researchers analyzing protected .NET code without being limited by trial expiration.

> ⚠️ For educational and research purposes only.


By https://t.me/dotnetreverse
