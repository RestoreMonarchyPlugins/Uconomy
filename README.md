# Uconomy
Add economy and in-game currency to your Unturned server.

## Credits
Plugin originally created by [fr34kyn01535](https://github.com/fr34kyn01535). We forked it to add some features and update libraries.

## Commands
- `/balance [balance]` - Shows your balance or another player's balance
- `/pay <player> <amount>` - Pay another player

## Permissions
```xml
<Permission Cooldown="0">balance</Permission>
<Permission Cooldown="0">balance.other</Permission>
<Permission Cooldown="0">bal</Permission>
<Permission Cooldown="0">pay</Permission>
```
## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<UconomyConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>yellow</MessageColor>
  <MessageIconUrl>https://i.imgur.com/dMDcc9J.png</MessageIconUrl>
  <DatabaseAddress>localhost</DatabaseAddress>
  <DatabaseUsername>unturned</DatabaseUsername>
  <DatabasePassword>password</DatabasePassword>
  <DatabaseName>unturned</DatabaseName>
  <DatabaseTableName>uconomy</DatabaseTableName>
  <DatabasePort>3306</DatabasePort>
  <InitialBalance>30</InitialBalance>
  <MoneyName>credits</MoneyName>
  <SyncExperience>false</SyncExperience>
  <SyncIntervalSeconds>5</SyncIntervalSeconds>
  <EnableSalaries>false</EnableSalaries>
  <SalaryIntervalSeconds>900</SalaryIntervalSeconds>
  <SalaryGroups>
    <SalaryGroup GroupId="default" Amount="10" />
    <SalaryGroup GroupId="vip" Amount="30" />
    <SalaryGroup GroupId="moderator" Amount="50" />
  </SalaryGroups>
</UconomyConfiguration>
```

## Translations
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="command_balance_show" Value="You have [[b]]{0} {1}[[/b]] in your account" />
  <Translation Id="command_balance_other" Value="[[b]]{0}[[/b]] has [[b]]{1} {2}[[/b]] in their account" />
  <Translation Id="command_balance_no_permission" Value="You don't have permission to check other players' balances" />
  <Translation Id="command_balance_player_not_found" Value="Couldn't find that player - try using their Steam64 ID instead" />
  <Translation Id="command_pay_invalid" Value="Please use /pay &lt;playerName&gt; &lt;amount&gt; to send money" />
  <Translation Id="command_pay_error_pay_self" Value="You can't send money to yourself!" />
  <Translation Id="command_pay_error_invalid_amount" Value="Please enter a valid amount greater than 0" />
  <Translation Id="command_pay_error_cant_afford" Value="You don't have enough money in your account for this payment" />
  <Translation Id="command_pay_error_player_not_found" Value="Couldn't find that player - make sure you typed their name correctly" />
  <Translation Id="command_pay_private" Value="You sent [[b]]{1} {2}[[/b]] to [[b]]{0}[[/b]]" />
  <Translation Id="command_pay_console" Value="You received [[b]]{0} {1}[[/b]] from the system" />
  <Translation Id="command_pay_other_private" Value="[[b]]{2}[[/b]] sent you [[b]]{0} {1}[[/b]]" />
  <Translation Id="salary_message" Value="You earned [[b]]{0} {1}[[/b]] for being [[b]]{2}[[/b]]" />
</Translations>
```