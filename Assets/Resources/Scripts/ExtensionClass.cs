using System;
public static class ExtensionClass {

	public static bool EqualsInValue(this float num1, float num2) {
		if (Math.Abs(num1 - num2) < float.Epsilon) {
			return true;
		}
		return false;
	}

	public static bool EqualsInValue(this double num1, float num2) {
		if (Math.Abs(num1 - num2) < double.Epsilon) {
			return true;
		}
		return false;
	}
}
