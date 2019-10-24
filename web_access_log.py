"""
Web Log Tool

This program analyses the web server access log from a
department-level Web server at the University of Calgary.

This trace contains approximately one year's worth of all HTTP requests to the
University of Calgary's Department of Computer Science web server. The hosts making requests to
the server have had their addresses removed to preserve privacy. Hosts are identified as either local
or remote where local is a host from the University of Calgary, and remote is a host from outside of
the University of Calgary domain. Paths have been removed. Files were numbered from 1 for the
first file encountered in the trace. Files retain the original file extension, so that the type of file can
be determined. Paths of the filenames have been removed. Modified filenames consist of two parts:
num.type, where num is a unique integer identifier, and type is the extension of the requested file.
Timestamps have 1 second resolution.

Author: Jacob Zaidi
"""

from datetime import datetime
from itertools import groupby
from operator import itemgetter
import matplotlib.pyplot as plt


class Parser:
    def __init__(self):
        self.numberOfDays = 353 # Count number of days passed
        
        self.fileTypeDict = {} # Contains file extension - file type information
        self.initializeFileType()
        
    def initializeFileType(self):  # Define file types for each file
        self.fileTypeDict["html"] = "HTML"
        self.fileTypeDict["htm"] = "HTML"
        self.fileTypeDict["shtml"] = "HTML"
        self.fileTypeDict["map"] = "HTML"

        self.fileTypeDict["gif"] = "Images"
        self.fileTypeDict["jpeg"] = "Images"
        self.fileTypeDict["jpg"] = "Images"
        self.fileTypeDict["xbm"] = "Images"
        self.fileTypeDict["bmp"] = "Images"
        self.fileTypeDict["rgb"] = "Images"
        self.fileTypeDict["xpm"] = "Images"

        self.fileTypeDict["au"] = "Sound"
        self.fileTypeDict["snd"] = "Sound"
        self.fileTypeDict["wav"] = "Sound"
        self.fileTypeDict["mid"] = "Sound"
        self.fileTypeDict["midi"] = "Sound"
        self.fileTypeDict["lha"] = "Sound"
        self.fileTypeDict["aif"] = "Sound"
        self.fileTypeDict["aiff"] = "Sound"

        self.fileTypeDict["mov"] = "Video"
        self.fileTypeDict["movie"] = "Video"
        self.fileTypeDict["avi"] = "Video"
        self.fileTypeDict["qt"] = "Video"
        self.fileTypeDict["mpeg"] = "Video"
        self.fileTypeDict["mpg"] = "Video"

        self.fileTypeDict["ps"] = "Formatted"
        self.fileTypeDict["eps"] = "Formatted"
        self.fileTypeDict["doc"] = "Formatted"
        self.fileTypeDict["dvi"] = "Formatted"
        self.fileTypeDict["txt"] = "Formatted"

        self.fileTypeDict["cgi"] = "Dynamic"
        self.fileTypeDict["pl"] = "Dynamic"
        self.fileTypeDict["cgi-bin"] = "Dynamic"

    def parse(self, logFile):  # Read each line from the log and process output
        index = 0
        request_count = 0
        successful = 0
        not_modified = 0
        found = 0
        unsuccessful = 0

        total_count = 0 
        total_byte = 0

        local_count = 0
        local_byte = 0
        remote_count = 0
        remote_byte = 0

        html_count = 0
        html_byte = 0
        images_count = 0
        images_byte = 0
        sound_count = 0
        sound_byte = 0
        video_count = 0
        video_byte = 0
        formatted_count = 0
        formatted_byte = 0
        dynamic_count = 0
        dynamic_byte = 0
        others_count = 0
        others_byte = 0

        object_list = []
        datetime_list = []

        hour_of_the_day = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]
        day_of_the_week = [0,0,0,0,0,0,0] # IMPORTANT, MONDAY = days[0]
        month_of_the_year = [0,0,0,0,0,0,0,0,0,0,0,0]

        
        for line in logFile:
            elements = line.split()

            # Skip to the next line if this line has an empty string
            if line is '':
                continue

            # Skip to the next line if this line contains not equal to 9 - 11 elements
            if not (9 <= len(elements) <= 11):
                continue

            # Corrects a record with a single "-"
            if (len(elements) == 9 and elements[2] != '-'):
                elements.insert(2, '-')

            # Breaks the record into useful information
            sourceAddress = elements[0]
            timeStr = elements[3].replace('[', '')
            requestMethod = elements[5]
            requestFileName = elements[6].replace('"', '')
            responseCode = elements[len(elements) - 2]
            replySizeInBytes = elements[len(elements) - 1]
            fileType = self.getFileType(requestFileName)

            # Checks the response code of the record
            if self.checkResCode(responseCode) == "Successful":
                successful += 1
            elif self.checkResCode(responseCode) == "Not Modified":
                not_modified += 1
            elif self.checkResCode(responseCode) == "Found":
                found += 1
            elif self.checkResCode(responseCode) == "Unsuccessful":
                unsuccessful += 1
            else:
                print("err")

            request_count += 1

            if replySizeInBytes == "-":
                continue

            # Checks if the record is from a local or remote source
            if sourceAddress == "local":
                local_count += 1
                local_byte += int(replySizeInBytes)
            elif sourceAddress == "remote":
                remote_count += 1
                remote_byte += int(replySizeInBytes)
            else:
                print("err")

            # Checks the file type of the record
            if fileType == "HTML":
                html_count += 1
                html_byte += int(replySizeInBytes)
            elif fileType == "Images":
                images_count += 1
                images_byte += int(replySizeInBytes)
            elif fileType == "Sound":
                sound_count += 1
                sound_byte += int(replySizeInBytes)
            elif fileType == "Video":
                video_count += 1
                video_byte += int(replySizeInBytes)
            elif fileType == "Formatted":
                formatted_count += 1
                formatted_byte += int(replySizeInBytes)
            elif fileType == "Dynamic":
                dynamic_count += 1
                dynamic_byte += int(replySizeInBytes)
            elif fileType == "Others":
                others_count += 1
                others_byte += int(replySizeInBytes)
            else:
                print("err")

            object_list.append((requestFileName, int(replySizeInBytes)))
            datetime_list.append((requestFileName, datetime.strptime(timeStr, "%d/%b/%Y:%H:%M:%S")))     

            date = datetime.strptime(timeStr, "%d/%b/%Y:%H:%M:%S")
            hour_of_the_day[date.hour] += 1
            day_of_the_week[date.weekday()] += 1
            month_of_the_year[date.month - 1] += 1  #date.month - 1 since January is 1 and December is 12


            total_count += 1
            total_byte += int(replySizeInBytes)
            
        # Outside the for loop, generate statistics output
        total_code = successful + not_modified + found + unsuccessful
        
        print("Q02. Requests made per day on average")
        print("{0:.2f}".format(request_count / self.numberOfDays))
        print()
        
        print("Q03. Bytes transferred during the entire log duration")
        print("{0:.2f} MB".format(total_byte / 1000000))
        print()

        print("Q04. Average number of bytes transferred per day")
        print("{0:.2f} MB".format(total_byte / (1000000 * self.numberOfDays)))
        print()

        print("Q05. Breakdown of server response codes")
        print("Successful: {0:.2f}%".format(100*successful/total_code))
        print("Not Modified: {0:.2f}%".format(100*not_modified/total_code))
        print("Found: {0:.2f}%".format(100*found/total_code))
        print("Unsuccessful: {0:.2f}%".format(100*unsuccessful/total_code))
        print()

        print("Q06. Breakdown of requests by each client category")
        print("Local: {0:.2f}%".format(100*local_count/total_count))
        print("Remote: {0:.2f}%".format(100*remote_count/total_count))
        print()
        
        print("Q07. Breakdown of bytes transferred by each client category")
        print("Local: {0:.2f}%".format(100*local_byte/total_byte))
        print("Remote: {0:.2f}%".format(100*remote_byte/total_byte))
        print()

        print("Q08. Breakdown of requests by file type category")
        print("HTML: {0:.2f}%".format(100*html_count/total_count))
        print("Images: {0:.2f}%".format(100*images_count/total_count))
        print("Sound: {0:.2f}%".format(100*sound_count/total_count))
        print("Video: {0:.2f}%".format(100*video_count/total_count))
        print("Formatted: {0:.2f}%".format(100*formatted_count/total_count))
        print("Dynamic: {0:.2f}%".format(100*dynamic_count/total_count))
        print("Others: {0:.2f}%".format(100*others_count/total_count))
        print()
        
        print("Q09. Breakdown of bytes transferred by each file category")
        print("HTML: {0:.2f}%".format(100*html_byte/total_byte))
        print("Images: {0:.2f}%".format(100*images_byte/total_byte))
        print("Sound: {0:.2f}%".format(100*sound_byte/total_byte))
        print("Video: {0:.2f}%".format(100*video_byte/total_byte))
        print("Formatted: {0:.2f}%".format(100*formatted_byte/total_byte))
        print("Dynamic: {0:.2f}%".format(100*dynamic_byte/total_byte))
        print("Others: {0:.2f}%".format(100*others_byte/total_byte))
        print()

        print("Q10. Average transfer sizes of each file category:")
        print("HTML: {0:.2f} B".format(html_byte/html_count))
        print("Images: {0:.2f} B".format(images_byte/images_count))
        print("Sound: {0:.2f} B".format(sound_byte/sound_count))
        print("Video: {0:.2f} B".format(video_byte/video_count))
        print("Formatted: {0:.2f} B".format(formatted_byte/formatted_count))
        print("Dynamic: {0:.2f} B".format(dynamic_byte/dynamic_count))
        print("Others: {0:.2f} B".format(others_byte/others_count))
        print()

        first = itemgetter(0)        
        sums_one = [(k, sum(item[1] for item in v))\
                for (k, v) in groupby(sorted(object_list, key=first), key=first)]

        sums_two = [(k, sum(1 for _ in v))\
                for (k, v) in groupby(sorted(object_list, key=first), key=first)]
        sums = []
        for i in range(len(sums_one)):
            if sums_one[i][0] == sums_two[i][0]:
                sums.append((sums_one[i][0],sums_two[i][1], sums_one[i][1]))
                
        total_unique_bytes = 0
        total_distinct = 0
        unique_bytes = 0
        unique_distinct =0 
        for k,t,v in sums:
            total_unique_bytes += v/t
            total_distinct += 1
            if t == 1:
                unique_bytes += v
                unique_distinct += 1

        bytes_percentage = 100*unique_bytes / total_unique_bytes
        objects_percentage = 100*unique_distinct / total_distinct
        print("Q11. Percentage of unique objects and bytes in the log")
        print("Unique Objects: {0:.2f}%".format(objects_percentage))
        print("Unique Bytes: {0:.2f}%".format(bytes_percentage))
        print()

        print("Q12. Producing plot... ", end="")
        q12_list1 = []
        q12_list2 = []
        for k,t,v in sums:
            q12_list1.append(v/t)
        q12_list1.sort()
        for i in range(len(q12_list1)):
            q12_list2.append((i+1)/len(q12_list1))

        plt.figure(1)
        plt.plot(q12_list1, q12_list2, '-o')
        plt.xscale("log")
        plt.suptitle('CDF Plot of Transfer Sizes of all Distinct Objects')
        plt.ylabel('CDF')
        plt.show(block=False)
        print("Complete!")

        print("Q13. Producing plots... ", end="")
        sum_hour_of_the_day = sum(hour_of_the_day)
        sum_day_of_the_week = sum(day_of_the_week)
        sum_month_of_the_year = sum(month_of_the_year)
        
        for i in range(len(hour_of_the_day)):
            hour_of_the_day[i] = 100*hour_of_the_day[i]/sum_hour_of_the_day
        for i in range(len(day_of_the_week)):
            day_of_the_week[i] = 100*day_of_the_week[i]/sum_day_of_the_week
        for i in range(len(month_of_the_year)):
            month_of_the_year[i] = 100*month_of_the_year[i]/sum_month_of_the_year

        hour_list = []
        for i in range(24):
            hour_list.append(i)
            
        day_list = []
        for i in range(7):
            day_list.append(i)
            
        month_list = []
        for i in range(12):
            month_list.append(i)

        plt.figure(2)
        plt.bar(hour_list, hour_of_the_day)
        plt.suptitle('Percentage of Total Requests per Hour of the Day')
        plt.show(block=False)
        print("Complete!")

        plt.figure(3)
        plt.bar(day_list, day_of_the_week)
        plt.suptitle('Percentage of Total Requests per Day of the Week')
        plt.show(block=False)

        plt.figure(4)
        plt.bar(month_list, month_of_the_year)
        plt.suptitle('Percentage of Total Requests per Month of the Year')
        plt.show(block=False)

        print("Q14. Producing plot... ", end="")
        q14_list1 = []
        q14_list2 = []
        sums_one = [(k, [item[1] for item in v])\
                for (k, v) in groupby(sorted(datetime_list, key=first), key=first)]

        sums_two = [(k, sum(1 for _ in v))\
                for (k, v) in groupby(sorted(datetime_list, key=first), key=first)]
        sums = []
        for i in range(len(sums_one)):
            if sums_one[i][0] == sums_two[i][0]:
                sums.append((sums_one[i][0],sums_two[i][1], sums_one[i][1]))
        for k,t,v in sums:
            v.sort()
            for i in range(len(v)-1):
                x = (v[i+1]-v[i]).total_seconds()
                q14_list1.append(x)
        q14_list1.sort()
        for i in range(len(q14_list1)):
            q14_list2.append((i+1)/len(q14_list1))
        plt.figure(5)
        plt.plot(q14_list1, q14_list2, '-o')
        plt.xscale("log")
        plt.suptitle('CDF Plot of Inter-reference Times')
        plt.ylabel('CDF')
        print("Complete!")
        print()
        plt.show()

    def getFileType(self, URI):
        if URI.endswith('/') or URI.endswith('.') or URI.endswith('..'):
            return 'HTML'
        filename = URI.split('/')[-1]
        if '?' in filename:
            return 'Dynamic'
        extension = filename.split('.')[-1].lower()
        if extension in self.fileTypeDict:
            return self.fileTypeDict[extension]
        return 'Others'

    def checkResCode(self, code):
        if code == '200' :
            return 'Successful'
        if code == '302' :
            return 'Found'
        if code == '304' :
            return 'Not Modified'   
        return 'Unsuccessful'

if __name__ == '__main__':
    logfile = open('access_log', 'r', errors='ignore')
    logParser = Parser()
    logParser.parse(logfile)
    pass
