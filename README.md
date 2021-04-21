# Overview

Simple application for reading log files in CSV format.

# Use

When open you will be prompted with a open file dialog to select a log directory and the application will monitor the changes in this directory and add the newly added entries to any .csv file.

You can filter the entries by level and regular expression.

There is rudimentary search and highlight.

# TODO

Known issues:
- Scroll is slow when there are >50k log entries. This is quite difficult to fix unless I disable the pixel scrolling. With pixel scrolling, if the height of the items is variable, the virtualizing stack panel is enumerating all items and scroll is sluggish.
- The performance improves greatly if you filter out large part of the items, for example, uncheck Notice
- it loads all items in memory, so if you open large log files it will take a lot of memory. I would not recommend loading too much data, I tried with a few hundred megabytes it takes a few seconds to load, but it works.
- when you scroll quickly the text gets garbled sometimes... It fixes automagically if you change the scroll position a bit. I think this is caused by WPF, I wasn't able to fix it.
- The main window is not brought into view after you select a directory. I need to fix that, it is annoying.

TODO:
- date filter
- clear button on regex boxes
- improve theme

Far future:
- Map on the scrollbar to show where the errors are
- Data virtualization (load only part of the log entries initially, load more when you scroll). This will make the performance with huge log files acceptable.



