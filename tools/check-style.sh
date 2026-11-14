#!/usr/bin/env bash
# Перевірки, які не покриває Roslyn: F-05, F-12, C-02, C-06, A-05, S-06, S-09, S-13.
export LC_ALL=C.UTF-8
fail=0

# check <файл> <код і опис> <регулярний вираз>
check() {
  local out
  out=$(tr -d '\r' < "$1" | grep -nE -- "$3")
  if [ -n "$out" ]; then
    while IFS= read -r hit; do
      echo "$1:${hit%%:*}: $2"
    done <<< "$out"
    fail=1
  fi
}

while IFS= read -r f; do
  check "$f" "F-05 рядок довший за 100 символів" '^.{101,}$'
  check "$f" "F-12 #region заборонено" '^[[:space:]]*#region'
  check "$f" "A-05 #pragma warning disable заборонено" '#pragma[[:space:]]+warning[[:space:]]+disable'
  check "$f" "C-06 TODO без автора й номера запису (TODO(ІМ, #12))" '(TODO|FIXME)([^(]|\([^)]*\)[^[:space:]]|\([^,)]*\))'
  check "$f" "C-02 закоментований код" '^[[:space:]]*//[[:space:]]*(if|for|foreach|while|var|return)[[:space:](].*[;{)][[:space:]]*$'
  case "$f" in
    ShopOrders.Core/*|ShopOrders.Domain/*)
      check "$f" "X-02 double/float заборонено в бібліотеках логіки" '(^|[^A-Za-z_])(double|float)([^A-Za-z_]|$)'
      ;;
  esac
  check "$f" "S-06 goto заборонено" '(^|[^A-Za-z_])goto[[:space:]]'
  check "$f" "S-09 порівняння з true/false" '(==|!=)[[:space:]]*(true|false)([^A-Za-z_]|$)'
  lines=$(wc -l < "$f")
  if [ "$lines" -gt 500 ]; then
    echo "$f:$lines: S-13 файл довший за 500 рядків"
    fail=1
  fi
done < <(git ls-files '*.cs' | grep -v -E '(^|/)(obj|bin)/')

if [ "$fail" -eq 0 ]; then echo "check-style: OK"; fi
exit "$fail"
