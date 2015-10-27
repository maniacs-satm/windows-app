Translation guide
================

## Easy
 Send me your e-mail adress and the language you want to add to [this e-mail adress](mailto:wallabag@jlnostr.de). This allows me to invite you to the project on [OneSkyApp](https://oneskyapp.com) where you can easily translate the app :)

# A bit harder
1. Fork the app.
2. Create folder `\src\wallabag.Universal\Strings\<language-code>`, for example `\src\wallabag.Universal\Strings\en-GB`.
3. Copy the file `Resources.resw` from the `en-US` folder to your newly created folder.
4. Ignore the first 120 lines, the interesting part should look similar to the code block at the end of this file.
5. Go through each data entry and translate the content between `<value>` and `</value>`.
6. Sometimes there's a comment under it. You don't need to translate it, it's just an information for you. For example `UPPERCASE` means that you should keep the translation in `UPPERCASE`, so e.g. instead of `funny` you transform it into the uppercase version, which looks like this: `FUNNY`.
7. So, you updated the app itself, but there's more. The store descriptions need to be translated, too. They are in `\store\App descriptions`. Copy the existing `en-US.md` file and rename it to the same language-code you specified in step 2.
8. Translate everything except the headings.
9. That's it. Submit a pull request and you're done :)

```xml
<data name="AddItemAppBarButton.Label" xml:space="preserve">
   <value>Add item</value>
</data>
```
