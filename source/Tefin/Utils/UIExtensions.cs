#region

using Avalonia;

#endregion

namespace Tefin.Utils;

public static class UIExtensions {
    public static T? FindParent<T>(this StyledElement? parent, Func<T, bool>? check = null) where T : StyledElement {
        if (parent == null) {
            return default;
        }

        if (parent is T matchedItem) {
            if (check == null) {
                return matchedItem;
            }

            return check(matchedItem) ? matchedItem : FindParent(matchedItem.Parent, check);
        }

        return FindParent(parent.Parent, check);
    }
}

public static class CollectionExtenstions {
    public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> check) {
        foreach (var i in source) {
            if (check(i)) {
                return true;
            }
        }

        return false;
    }
}