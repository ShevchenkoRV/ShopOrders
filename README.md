# ShopOrders

![CI](https://github.com/ShevchenkoRV/ShopOrders/actions/workflows/ci.yml/badge.svg)

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».
Варіант № 1: облік замовлень інтернет-магазину.

## Склад рішення

- ShopOrders.Core — логіка предметної області (бібліотека)
- ShopOrders.App — консольний застосунок (введення, виведення)

## Вимоги

.NET SDK 8.0 або новіший, Git.

## Складання та запуск

    dotnet build
    dotnet run --project ShopOrders.App

## Стандарт кодування

Правила описано у файлі CODING_STANDARD.md і закріплено
в .editorconfig. Перевірка: dotnet format --verify-no-changes

## Автор

Шевченко Ростислав Володимирович, група 472

## Конвеєр CI

На кожен push у `main` та `feature/**` і на кожен pull request у `main` виконується:

1. `dotnet format --verify-no-changes` - форматування;
2. `dotnet build -c Release` - складання без попереджень (TreatWarningsAsErrors);
3. `dotnet test` - тести;
4. покриття рядків не нижче 60 %.
