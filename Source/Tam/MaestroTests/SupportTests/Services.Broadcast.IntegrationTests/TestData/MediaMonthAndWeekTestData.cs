using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class MediaMonthAndWeekTestData
    {
        public static List<MediaWeek> GetMediaWeeksIntersecting(DateTime start, DateTime end)
        {
            var result = _MediaWeeks.Where(w => w.StartDate <= end && w.EndDate >= start)
                .OrderBy(m => m.StartDate).ToList();
            return result;
        }

        public static List<DisplayMediaWeek> GetDisplayMediaWeekByFlight(DateTime start, DateTime end)
        {
            var mediaWeeks = GetMediaWeeksIntersecting(start, end);

            var result = mediaWeeks.Select(w =>
            {
                var month = GetMediaMonthById(w.MediaMonthId);
                var week = new DisplayMediaWeek
                {
                    Id = w.Id,
                    WeekStartDate = w.StartDate,
                    WeekEndDate = w.EndDate,
                    MediaMonthId = w.MediaMonthId,
                    Year = month.Year,
                    Month = month.Month,
                    Week = w.WeekNumber,
                    MonthStartDate = month.StartDate,
                    MonthEndDate = month.EndDate
                };
                return week;
            }).ToList();

            return result;
        }

        public static List<MediaMonth> GetMediaMonthsIntersecting(DateTime start, DateTime end)
        {
            var result = _MediaMonths.Where(w => w.StartDate <= end && w.EndDate >= start)
                .OrderBy(m => m.StartDate).ToList();
            return result;
        }

        public static MediaMonth GetMediaMonthById(int id)
        {
            var result = _MediaMonths.First(m => m.Id == id);
            return result;
        }

        public static List<MediaWeek> GetMediaWeeksByMediaMonth(int mediaMonthId)
        {
            var result = _MediaWeeks.Where(w => w.MediaMonthId == mediaMonthId)
                .OrderBy(m => m.StartDate).ToList();
            return result;
        }

        public static MediaWeek GetMediaWeek(int mediaWeekId)
        {
            var result = _MediaWeeks.FirstOrDefault(w => w.Id == mediaWeekId);
            return result;
        }

        #region Big Lists

        private static List<MediaWeek> _MediaWeeks = new List<MediaWeek>
        {
            new MediaWeek(888, 472, 1, DateTime.Parse("Dec 28 2020 12:00AM"), DateTime.Parse("Jan  3 2021 12:00AM")),
            new MediaWeek(887, 471, 4, DateTime.Parse("Dec 21 2020 12:00AM"), DateTime.Parse("Dec 27 2020 12:00AM")),
            new MediaWeek(886, 471, 3, DateTime.Parse("Dec 14 2020 12:00AM"), DateTime.Parse("Dec 20 2020 12:00AM")),
            new MediaWeek(885, 471, 2, DateTime.Parse("Dec  7 2020 12:00AM"), DateTime.Parse("Dec 13 2020 12:00AM")),
            new MediaWeek(884, 471, 1, DateTime.Parse("Nov 30 2020 12:00AM"), DateTime.Parse("Dec  6 2020 12:00AM")),
            new MediaWeek(883, 470, 5, DateTime.Parse("Nov 23 2020 12:00AM"), DateTime.Parse("Nov 29 2020 12:00AM")),
            new MediaWeek(882, 470, 4, DateTime.Parse("Nov 16 2020 12:00AM"), DateTime.Parse("Nov 22 2020 12:00AM")),
            new MediaWeek(881, 470, 3, DateTime.Parse("Nov  9 2020 12:00AM"), DateTime.Parse("Nov 15 2020 12:00AM")),
            new MediaWeek(880, 470, 2, DateTime.Parse("Nov  2 2020 12:00AM"), DateTime.Parse("Nov  8 2020 12:00AM")),
            new MediaWeek(879, 470, 1, DateTime.Parse("Oct 26 2020 12:00AM"), DateTime.Parse("Nov  1 2020 12:00AM")),
            new MediaWeek(878, 469, 4, DateTime.Parse("Oct 19 2020 12:00AM"), DateTime.Parse("Oct 25 2020 12:00AM")),
            new MediaWeek(877, 469, 3, DateTime.Parse("Oct 12 2020 12:00AM"), DateTime.Parse("Oct 18 2020 12:00AM")),
            new MediaWeek(876, 469, 2, DateTime.Parse("Oct  5 2020 12:00AM"), DateTime.Parse("Oct 11 2020 12:00AM")),
            new MediaWeek(875, 469, 1, DateTime.Parse("Sep 28 2020 12:00AM"), DateTime.Parse("Oct  4 2020 12:00AM")),
            new MediaWeek(874, 468, 4, DateTime.Parse("Sep 21 2020 12:00AM"), DateTime.Parse("Sep 27 2020 12:00AM")),
            new MediaWeek(873, 468, 3, DateTime.Parse("Sep 14 2020 12:00AM"), DateTime.Parse("Sep 20 2020 12:00AM")),
            new MediaWeek(872, 468, 2, DateTime.Parse("Sep  7 2020 12:00AM"), DateTime.Parse("Sep 13 2020 12:00AM")),
            new MediaWeek(871, 468, 1, DateTime.Parse("Aug 31 2020 12:00AM"), DateTime.Parse("Sep  6 2020 12:00AM")),
            new MediaWeek(870, 467, 5, DateTime.Parse("Aug 24 2020 12:00AM"), DateTime.Parse("Aug 30 2020 12:00AM")),
            new MediaWeek(869, 467, 4, DateTime.Parse("Aug 17 2020 12:00AM"), DateTime.Parse("Aug 23 2020 12:00AM")),
            new MediaWeek(868, 467, 3, DateTime.Parse("Aug 10 2020 12:00AM"), DateTime.Parse("Aug 16 2020 12:00AM")),
            new MediaWeek(867, 467, 2, DateTime.Parse("Aug  3 2020 12:00AM"), DateTime.Parse("Aug  9 2020 12:00AM")),
            new MediaWeek(866, 467, 1, DateTime.Parse("Jul 27 2020 12:00AM"), DateTime.Parse("Aug  2 2020 12:00AM")),
            new MediaWeek(865, 466, 4, DateTime.Parse("Jul 20 2020 12:00AM"), DateTime.Parse("Jul 26 2020 12:00AM")),
            new MediaWeek(864, 466, 3, DateTime.Parse("Jul 13 2020 12:00AM"), DateTime.Parse("Jul 19 2020 12:00AM")),
            new MediaWeek(863, 466, 2, DateTime.Parse("Jul  6 2020 12:00AM"), DateTime.Parse("Jul 12 2020 12:00AM")),
            new MediaWeek(862, 466, 1, DateTime.Parse("Jun 29 2020 12:00AM"), DateTime.Parse("Jul  5 2020 12:00AM")),
            new MediaWeek(861, 465, 4, DateTime.Parse("Jun 22 2020 12:00AM"), DateTime.Parse("Jun 28 2020 12:00AM")),
            new MediaWeek(860, 465, 3, DateTime.Parse("Jun 15 2020 12:00AM"), DateTime.Parse("Jun 21 2020 12:00AM")),
            new MediaWeek(859, 465, 2, DateTime.Parse("Jun  8 2020 12:00AM"), DateTime.Parse("Jun 14 2020 12:00AM")),
            new MediaWeek(858, 465, 1, DateTime.Parse("Jun  1 2020 12:00AM"), DateTime.Parse("Jun  7 2020 12:00AM")),
            new MediaWeek(857, 464, 5, DateTime.Parse("May 25 2020 12:00AM"), DateTime.Parse("May 31 2020 12:00AM")),
            new MediaWeek(856, 464, 4, DateTime.Parse("May 18 2020 12:00AM"), DateTime.Parse("May 24 2020 12:00AM")),
            new MediaWeek(855, 464, 3, DateTime.Parse("May 11 2020 12:00AM"), DateTime.Parse("May 17 2020 12:00AM")),
            new MediaWeek(854, 464, 2, DateTime.Parse("May  4 2020 12:00AM"), DateTime.Parse("May 10 2020 12:00AM")),
            new MediaWeek(853, 464, 1, DateTime.Parse("Apr 27 2020 12:00AM"), DateTime.Parse("May  3 2020 12:00AM")),
            new MediaWeek(852, 463, 4, DateTime.Parse("Apr 20 2020 12:00AM"), DateTime.Parse("Apr 26 2020 12:00AM")),
            new MediaWeek(851, 463, 3, DateTime.Parse("Apr 13 2020 12:00AM"), DateTime.Parse("Apr 19 2020 12:00AM")),
            new MediaWeek(850, 463, 2, DateTime.Parse("Apr  6 2020 12:00AM"), DateTime.Parse("Apr 12 2020 12:00AM")),
            new MediaWeek(849, 463, 1, DateTime.Parse("Mar 30 2020 12:00AM"), DateTime.Parse("Apr  5 2020 12:00AM")),
            new MediaWeek(848, 462, 5, DateTime.Parse("Mar 23 2020 12:00AM"), DateTime.Parse("Mar 29 2020 12:00AM")),
            new MediaWeek(847, 462, 4, DateTime.Parse("Mar 16 2020 12:00AM"), DateTime.Parse("Mar 22 2020 12:00AM")),
            new MediaWeek(846, 462, 3, DateTime.Parse("Mar  9 2020 12:00AM"), DateTime.Parse("Mar 15 2020 12:00AM")),
            new MediaWeek(845, 462, 2, DateTime.Parse("Mar  2 2020 12:00AM"), DateTime.Parse("Mar  8 2020 12:00AM")),
            new MediaWeek(844, 462, 1, DateTime.Parse("Feb 24 2020 12:00AM"), DateTime.Parse("Mar  1 2020 12:00AM")),
            new MediaWeek(843, 461, 4, DateTime.Parse("Feb 17 2020 12:00AM"), DateTime.Parse("Feb 23 2020 12:00AM")),
            new MediaWeek(842, 461, 3, DateTime.Parse("Feb 10 2020 12:00AM"), DateTime.Parse("Feb 16 2020 12:00AM")),
            new MediaWeek(841, 461, 2, DateTime.Parse("Feb  3 2020 12:00AM"), DateTime.Parse("Feb  9 2020 12:00AM")),
            new MediaWeek(840, 461, 1, DateTime.Parse("Jan 27 2020 12:00AM"), DateTime.Parse("Feb  2 2020 12:00AM")),
            new MediaWeek(839, 460, 4, DateTime.Parse("Jan 20 2020 12:00AM"), DateTime.Parse("Jan 26 2020 12:00AM")),
            new MediaWeek(838, 460, 3, DateTime.Parse("Jan 13 2020 12:00AM"), DateTime.Parse("Jan 19 2020 12:00AM")),
            new MediaWeek(837, 460, 2, DateTime.Parse("Jan  6 2020 12:00AM"), DateTime.Parse("Jan 12 2020 12:00AM")),
            new MediaWeek(836, 460, 1, DateTime.Parse("Dec 30 2019 12:00AM"), DateTime.Parse("Jan  5 2020 12:00AM")),
            new MediaWeek(835, 459, 5, DateTime.Parse("Dec 23 2019 12:00AM"), DateTime.Parse("Dec 29 2019 12:00AM")),
            new MediaWeek(834, 459, 4, DateTime.Parse("Dec 16 2019 12:00AM"), DateTime.Parse("Dec 22 2019 12:00AM")),
            new MediaWeek(833, 459, 3, DateTime.Parse("Dec  9 2019 12:00AM"), DateTime.Parse("Dec 15 2019 12:00AM")),
            new MediaWeek(832, 459, 2, DateTime.Parse("Dec  2 2019 12:00AM"), DateTime.Parse("Dec  8 2019 12:00AM")),
            new MediaWeek(831, 459, 1, DateTime.Parse("Nov 25 2019 12:00AM"), DateTime.Parse("Dec  1 2019 12:00AM")),
            new MediaWeek(830, 458, 4, DateTime.Parse("Nov 18 2019 12:00AM"), DateTime.Parse("Nov 24 2019 12:00AM")),
            new MediaWeek(829, 458, 3, DateTime.Parse("Nov 11 2019 12:00AM"), DateTime.Parse("Nov 17 2019 12:00AM")),
            new MediaWeek(828, 458, 2, DateTime.Parse("Nov  4 2019 12:00AM"), DateTime.Parse("Nov 10 2019 12:00AM")),
            new MediaWeek(827, 458, 1, DateTime.Parse("Oct 28 2019 12:00AM"), DateTime.Parse("Nov  3 2019 12:00AM")),
            new MediaWeek(826, 457, 4, DateTime.Parse("Oct 21 2019 12:00AM"), DateTime.Parse("Oct 27 2019 12:00AM")),
            new MediaWeek(825, 457, 3, DateTime.Parse("Oct 14 2019 12:00AM"), DateTime.Parse("Oct 20 2019 12:00AM")),
            new MediaWeek(824, 457, 2, DateTime.Parse("Oct  7 2019 12:00AM"), DateTime.Parse("Oct 13 2019 12:00AM")),
            new MediaWeek(823, 457, 1, DateTime.Parse("Sep 30 2019 12:00AM"), DateTime.Parse("Oct  6 2019 12:00AM")),
            new MediaWeek(822, 456, 5, DateTime.Parse("Sep 23 2019 12:00AM"), DateTime.Parse("Sep 29 2019 12:00AM")),
            new MediaWeek(821, 456, 4, DateTime.Parse("Sep 16 2019 12:00AM"), DateTime.Parse("Sep 22 2019 12:00AM")),
            new MediaWeek(820, 456, 3, DateTime.Parse("Sep  9 2019 12:00AM"), DateTime.Parse("Sep 15 2019 12:00AM")),
            new MediaWeek(819, 456, 2, DateTime.Parse("Sep  2 2019 12:00AM"), DateTime.Parse("Sep  8 2019 12:00AM")),
            new MediaWeek(818, 456, 1, DateTime.Parse("Aug 26 2019 12:00AM"), DateTime.Parse("Sep  1 2019 12:00AM")),
            new MediaWeek(817, 455, 4, DateTime.Parse("Aug 19 2019 12:00AM"), DateTime.Parse("Aug 25 2019 12:00AM")),
            new MediaWeek(816, 455, 3, DateTime.Parse("Aug 12 2019 12:00AM"), DateTime.Parse("Aug 18 2019 12:00AM")),
            new MediaWeek(815, 455, 2, DateTime.Parse("Aug  5 2019 12:00AM"), DateTime.Parse("Aug 11 2019 12:00AM")),
            new MediaWeek(814, 455, 1, DateTime.Parse("Jul 29 2019 12:00AM"), DateTime.Parse("Aug  4 2019 12:00AM")),
            new MediaWeek(813, 454, 4, DateTime.Parse("Jul 22 2019 12:00AM"), DateTime.Parse("Jul 28 2019 12:00AM")),
            new MediaWeek(812, 454, 3, DateTime.Parse("Jul 15 2019 12:00AM"), DateTime.Parse("Jul 21 2019 12:00AM")),
            new MediaWeek(811, 454, 2, DateTime.Parse("Jul  8 2019 12:00AM"), DateTime.Parse("Jul 14 2019 12:00AM")),
            new MediaWeek(810, 454, 1, DateTime.Parse("Jul  1 2019 12:00AM"), DateTime.Parse("Jul  7 2019 12:00AM")),
            new MediaWeek(809, 453, 5, DateTime.Parse("Jun 24 2019 12:00AM"), DateTime.Parse("Jun 30 2019 12:00AM")),
            new MediaWeek(808, 453, 4, DateTime.Parse("Jun 17 2019 12:00AM"), DateTime.Parse("Jun 23 2019 12:00AM")),
            new MediaWeek(807, 453, 3, DateTime.Parse("Jun 10 2019 12:00AM"), DateTime.Parse("Jun 16 2019 12:00AM")),
            new MediaWeek(806, 453, 2, DateTime.Parse("Jun  3 2019 12:00AM"), DateTime.Parse("Jun  9 2019 12:00AM")),
            new MediaWeek(805, 453, 1, DateTime.Parse("May 27 2019 12:00AM"), DateTime.Parse("Jun  2 2019 12:00AM")),
            new MediaWeek(804, 452, 4, DateTime.Parse("May 20 2019 12:00AM"), DateTime.Parse("May 26 2019 12:00AM")),
            new MediaWeek(803, 452, 3, DateTime.Parse("May 13 2019 12:00AM"), DateTime.Parse("May 19 2019 12:00AM")),
            new MediaWeek(802, 452, 2, DateTime.Parse("May  6 2019 12:00AM"), DateTime.Parse("May 12 2019 12:00AM")),
            new MediaWeek(801, 452, 1, DateTime.Parse("Apr 29 2019 12:00AM"), DateTime.Parse("May  5 2019 12:00AM")),
            new MediaWeek(800, 451, 4, DateTime.Parse("Apr 22 2019 12:00AM"), DateTime.Parse("Apr 28 2019 12:00AM")),
            new MediaWeek(799, 451, 3, DateTime.Parse("Apr 15 2019 12:00AM"), DateTime.Parse("Apr 21 2019 12:00AM")),
            new MediaWeek(798, 451, 2, DateTime.Parse("Apr  8 2019 12:00AM"), DateTime.Parse("Apr 14 2019 12:00AM")),
            new MediaWeek(797, 451, 1, DateTime.Parse("Apr  1 2019 12:00AM"), DateTime.Parse("Apr  7 2019 12:00AM")),
            new MediaWeek(796, 450, 5, DateTime.Parse("Mar 25 2019 12:00AM"), DateTime.Parse("Mar 31 2019 12:00AM")),
            new MediaWeek(795, 450, 4, DateTime.Parse("Mar 18 2019 12:00AM"), DateTime.Parse("Mar 24 2019 12:00AM")),
            new MediaWeek(794, 450, 3, DateTime.Parse("Mar 11 2019 12:00AM"), DateTime.Parse("Mar 17 2019 12:00AM")),
            new MediaWeek(793, 450, 2, DateTime.Parse("Mar  4 2019 12:00AM"), DateTime.Parse("Mar 10 2019 12:00AM")),
            new MediaWeek(792, 450, 1, DateTime.Parse("Feb 25 2019 12:00AM"), DateTime.Parse("Mar  3 2019 12:00AM")),
            new MediaWeek(791, 449, 4, DateTime.Parse("Feb 18 2019 12:00AM"), DateTime.Parse("Feb 24 2019 12:00AM")),
            new MediaWeek(790, 449, 3, DateTime.Parse("Feb 11 2019 12:00AM"), DateTime.Parse("Feb 17 2019 12:00AM")),
            new MediaWeek(789, 449, 2, DateTime.Parse("Feb  4 2019 12:00AM"), DateTime.Parse("Feb 10 2019 12:00AM")),
            new MediaWeek(788, 449, 1, DateTime.Parse("Jan 28 2019 12:00AM"), DateTime.Parse("Feb  3 2019 12:00AM")),
            new MediaWeek(787, 448, 4, DateTime.Parse("Jan 21 2019 12:00AM"), DateTime.Parse("Jan 27 2019 12:00AM")),
            new MediaWeek(786, 448, 3, DateTime.Parse("Jan 14 2019 12:00AM"), DateTime.Parse("Jan 20 2019 12:00AM")),
            new MediaWeek(785, 448, 2, DateTime.Parse("Jan  7 2019 12:00AM"), DateTime.Parse("Jan 13 2019 12:00AM")),
            new MediaWeek(784, 448, 1, DateTime.Parse("Dec 31 2018 12:00AM"), DateTime.Parse("Jan  6 2019 12:00AM")),
            new MediaWeek(783, 447, 5, DateTime.Parse("Dec 24 2018 12:00AM"), DateTime.Parse("Dec 30 2018 12:00AM")),
            new MediaWeek(782, 447, 4, DateTime.Parse("Dec 17 2018 12:00AM"), DateTime.Parse("Dec 23 2018 12:00AM")),
            new MediaWeek(781, 447, 3, DateTime.Parse("Dec 10 2018 12:00AM"), DateTime.Parse("Dec 16 2018 12:00AM")),
            new MediaWeek(780, 447, 2, DateTime.Parse("Dec  3 2018 12:00AM"), DateTime.Parse("Dec  9 2018 12:00AM")),
            new MediaWeek(779, 447, 1, DateTime.Parse("Nov 26 2018 12:00AM"), DateTime.Parse("Dec  2 2018 12:00AM")),
            new MediaWeek(778, 446, 4, DateTime.Parse("Nov 19 2018 12:00AM"), DateTime.Parse("Nov 25 2018 12:00AM")),
            new MediaWeek(777, 446, 3, DateTime.Parse("Nov 12 2018 12:00AM"), DateTime.Parse("Nov 18 2018 12:00AM")),
            new MediaWeek(776, 446, 2, DateTime.Parse("Nov  5 2018 12:00AM"), DateTime.Parse("Nov 11 2018 12:00AM")),
            new MediaWeek(775, 446, 1, DateTime.Parse("Oct 29 2018 12:00AM"), DateTime.Parse("Nov  4 2018 12:00AM")),
            new MediaWeek(774, 445, 4, DateTime.Parse("Oct 22 2018 12:00AM"), DateTime.Parse("Oct 28 2018 12:00AM")),
            new MediaWeek(773, 445, 3, DateTime.Parse("Oct 15 2018 12:00AM"), DateTime.Parse("Oct 21 2018 12:00AM")),
            new MediaWeek(772, 445, 2, DateTime.Parse("Oct  8 2018 12:00AM"), DateTime.Parse("Oct 14 2018 12:00AM")),
            new MediaWeek(771, 445, 1, DateTime.Parse("Oct  1 2018 12:00AM"), DateTime.Parse("Oct  7 2018 12:00AM")),
            new MediaWeek(770, 444, 5, DateTime.Parse("Sep 24 2018 12:00AM"), DateTime.Parse("Sep 30 2018 12:00AM")),
            new MediaWeek(769, 444, 4, DateTime.Parse("Sep 17 2018 12:00AM"), DateTime.Parse("Sep 23 2018 12:00AM")),
            new MediaWeek(768, 444, 3, DateTime.Parse("Sep 10 2018 12:00AM"), DateTime.Parse("Sep 16 2018 12:00AM")),
            new MediaWeek(767, 444, 2, DateTime.Parse("Sep  3 2018 12:00AM"), DateTime.Parse("Sep  9 2018 12:00AM")),
            new MediaWeek(766, 444, 1, DateTime.Parse("Aug 27 2018 12:00AM"), DateTime.Parse("Sep  2 2018 12:00AM")),
            new MediaWeek(765, 443, 4, DateTime.Parse("Aug 20 2018 12:00AM"), DateTime.Parse("Aug 26 2018 12:00AM")),
            new MediaWeek(764, 443, 3, DateTime.Parse("Aug 13 2018 12:00AM"), DateTime.Parse("Aug 19 2018 12:00AM")),
            new MediaWeek(763, 443, 2, DateTime.Parse("Aug  6 2018 12:00AM"), DateTime.Parse("Aug 12 2018 12:00AM")),
            new MediaWeek(762, 443, 1, DateTime.Parse("Jul 30 2018 12:00AM"), DateTime.Parse("Aug  5 2018 12:00AM")),
            new MediaWeek(761, 442, 5, DateTime.Parse("Jul 23 2018 12:00AM"), DateTime.Parse("Jul 29 2018 12:00AM")),
            new MediaWeek(760, 442, 4, DateTime.Parse("Jul 16 2018 12:00AM"), DateTime.Parse("Jul 22 2018 12:00AM")),
            new MediaWeek(759, 442, 3, DateTime.Parse("Jul  9 2018 12:00AM"), DateTime.Parse("Jul 15 2018 12:00AM")),
            new MediaWeek(758, 442, 2, DateTime.Parse("Jul  2 2018 12:00AM"), DateTime.Parse("Jul  8 2018 12:00AM")),
            new MediaWeek(757, 442, 1, DateTime.Parse("Jun 25 2018 12:00AM"), DateTime.Parse("Jul  1 2018 12:00AM")),
            new MediaWeek(756, 441, 4, DateTime.Parse("Jun 18 2018 12:00AM"), DateTime.Parse("Jun 24 2018 12:00AM")),
            new MediaWeek(755, 441, 3, DateTime.Parse("Jun 11 2018 12:00AM"), DateTime.Parse("Jun 17 2018 12:00AM")),
            new MediaWeek(754, 441, 2, DateTime.Parse("Jun  4 2018 12:00AM"), DateTime.Parse("Jun 10 2018 12:00AM")),
            new MediaWeek(753, 441, 1, DateTime.Parse("May 28 2018 12:00AM"), DateTime.Parse("Jun  3 2018 12:00AM")),
            new MediaWeek(752, 440, 4, DateTime.Parse("May 21 2018 12:00AM"), DateTime.Parse("May 27 2018 12:00AM")),
            new MediaWeek(751, 440, 3, DateTime.Parse("May 14 2018 12:00AM"), DateTime.Parse("May 20 2018 12:00AM")),
            new MediaWeek(750, 440, 2, DateTime.Parse("May  7 2018 12:00AM"), DateTime.Parse("May 13 2018 12:00AM")),
            new MediaWeek(749, 440, 1, DateTime.Parse("Apr 30 2018 12:00AM"), DateTime.Parse("May  6 2018 12:00AM")),
            new MediaWeek(748, 439, 5, DateTime.Parse("Apr 23 2018 12:00AM"), DateTime.Parse("Apr 29 2018 12:00AM")),
            new MediaWeek(747, 439, 4, DateTime.Parse("Apr 16 2018 12:00AM"), DateTime.Parse("Apr 22 2018 12:00AM")),
            new MediaWeek(746, 439, 3, DateTime.Parse("Apr  9 2018 12:00AM"), DateTime.Parse("Apr 15 2018 12:00AM")),
            new MediaWeek(745, 439, 2, DateTime.Parse("Apr  2 2018 12:00AM"), DateTime.Parse("Apr  8 2018 12:00AM")),
            new MediaWeek(744, 439, 1, DateTime.Parse("Mar 26 2018 12:00AM"), DateTime.Parse("Apr  1 2018 12:00AM")),
            new MediaWeek(743, 438, 4, DateTime.Parse("Mar 19 2018 12:00AM"), DateTime.Parse("Mar 25 2018 12:00AM")),
            new MediaWeek(742, 438, 3, DateTime.Parse("Mar 12 2018 12:00AM"), DateTime.Parse("Mar 18 2018 12:00AM")),
            new MediaWeek(741, 438, 2, DateTime.Parse("Mar  5 2018 12:00AM"), DateTime.Parse("Mar 11 2018 12:00AM")),
            new MediaWeek(740, 438, 1, DateTime.Parse("Feb 26 2018 12:00AM"), DateTime.Parse("Mar  4 2018 12:00AM")),
            new MediaWeek(739, 437, 4, DateTime.Parse("Feb 19 2018 12:00AM"), DateTime.Parse("Feb 25 2018 12:00AM")),
            new MediaWeek(738, 437, 3, DateTime.Parse("Feb 12 2018 12:00AM"), DateTime.Parse("Feb 18 2018 12:00AM")),
            new MediaWeek(737, 437, 2, DateTime.Parse("Feb  5 2018 12:00AM"), DateTime.Parse("Feb 11 2018 12:00AM")),
            new MediaWeek(736, 437, 1, DateTime.Parse("Jan 29 2018 12:00AM"), DateTime.Parse("Feb  4 2018 12:00AM")),
            new MediaWeek(735, 436, 4, DateTime.Parse("Jan 22 2018 12:00AM"), DateTime.Parse("Jan 28 2018 12:00AM")),
            new MediaWeek(734, 436, 3, DateTime.Parse("Jan 15 2018 12:00AM"), DateTime.Parse("Jan 21 2018 12:00AM")),
            new MediaWeek(733, 436, 2, DateTime.Parse("Jan  8 2018 12:00AM"), DateTime.Parse("Jan 14 2018 12:00AM")),
            new MediaWeek(732, 436, 1, DateTime.Parse("Jan  1 2018 12:00AM"), DateTime.Parse("Jan  7 2018 12:00AM")),
            new MediaWeek(731, 435, 5, DateTime.Parse("Dec 25 2017 12:00AM"), DateTime.Parse("Dec 31 2017 12:00AM")),
            new MediaWeek(730, 435, 4, DateTime.Parse("Dec 18 2017 12:00AM"), DateTime.Parse("Dec 24 2017 12:00AM")),
            new MediaWeek(729, 435, 3, DateTime.Parse("Dec 11 2017 12:00AM"), DateTime.Parse("Dec 17 2017 12:00AM")),
            new MediaWeek(728, 435, 2, DateTime.Parse("Dec  4 2017 12:00AM"), DateTime.Parse("Dec 10 2017 12:00AM")),
            new MediaWeek(727, 435, 1, DateTime.Parse("Nov 27 2017 12:00AM"), DateTime.Parse("Dec  3 2017 12:00AM")),
            new MediaWeek(726, 434, 4, DateTime.Parse("Nov 20 2017 12:00AM"), DateTime.Parse("Nov 26 2017 12:00AM")),
            new MediaWeek(725, 434, 3, DateTime.Parse("Nov 13 2017 12:00AM"), DateTime.Parse("Nov 19 2017 12:00AM")),
            new MediaWeek(724, 434, 2, DateTime.Parse("Nov  6 2017 12:00AM"), DateTime.Parse("Nov 12 2017 12:00AM")),
            new MediaWeek(723, 434, 1, DateTime.Parse("Oct 30 2017 12:00AM"), DateTime.Parse("Nov  5 2017 12:00AM")),
            new MediaWeek(722, 433, 5, DateTime.Parse("Oct 23 2017 12:00AM"), DateTime.Parse("Oct 29 2017 12:00AM")),
            new MediaWeek(721, 433, 4, DateTime.Parse("Oct 16 2017 12:00AM"), DateTime.Parse("Oct 22 2017 12:00AM")),
            new MediaWeek(720, 433, 3, DateTime.Parse("Oct  9 2017 12:00AM"), DateTime.Parse("Oct 15 2017 12:00AM")),
            new MediaWeek(719, 433, 2, DateTime.Parse("Oct  2 2017 12:00AM"), DateTime.Parse("Oct  8 2017 12:00AM")),
            new MediaWeek(718, 433, 1, DateTime.Parse("Sep 25 2017 12:00AM"), DateTime.Parse("Oct  1 2017 12:00AM")),
            new MediaWeek(717, 432, 4, DateTime.Parse("Sep 18 2017 12:00AM"), DateTime.Parse("Sep 24 2017 12:00AM")),
            new MediaWeek(716, 432, 3, DateTime.Parse("Sep 11 2017 12:00AM"), DateTime.Parse("Sep 17 2017 12:00AM")),
            new MediaWeek(715, 432, 2, DateTime.Parse("Sep  4 2017 12:00AM"), DateTime.Parse("Sep 10 2017 12:00AM")),
            new MediaWeek(714, 432, 1, DateTime.Parse("Aug 28 2017 12:00AM"), DateTime.Parse("Sep  3 2017 12:00AM")),
            new MediaWeek(713, 431, 4, DateTime.Parse("Aug 21 2017 12:00AM"), DateTime.Parse("Aug 27 2017 12:00AM")),
            new MediaWeek(712, 431, 3, DateTime.Parse("Aug 14 2017 12:00AM"), DateTime.Parse("Aug 20 2017 12:00AM")),
            new MediaWeek(711, 431, 2, DateTime.Parse("Aug  7 2017 12:00AM"), DateTime.Parse("Aug 13 2017 12:00AM")),
            new MediaWeek(710, 431, 1, DateTime.Parse("Jul 31 2017 12:00AM"), DateTime.Parse("Aug  6 2017 12:00AM")),
            new MediaWeek(709, 430, 5, DateTime.Parse("Jul 24 2017 12:00AM"), DateTime.Parse("Jul 30 2017 12:00AM")),
            new MediaWeek(708, 430, 4, DateTime.Parse("Jul 17 2017 12:00AM"), DateTime.Parse("Jul 23 2017 12:00AM")),
            new MediaWeek(707, 430, 3, DateTime.Parse("Jul 10 2017 12:00AM"), DateTime.Parse("Jul 16 2017 12:00AM")),
            new MediaWeek(706, 430, 2, DateTime.Parse("Jul  3 2017 12:00AM"), DateTime.Parse("Jul  9 2017 12:00AM")),
            new MediaWeek(705, 430, 1, DateTime.Parse("Jun 26 2017 12:00AM"), DateTime.Parse("Jul  2 2017 12:00AM")),
            new MediaWeek(704, 429, 4, DateTime.Parse("Jun 19 2017 12:00AM"), DateTime.Parse("Jun 25 2017 12:00AM")),
            new MediaWeek(703, 429, 3, DateTime.Parse("Jun 12 2017 12:00AM"), DateTime.Parse("Jun 18 2017 12:00AM")),
            new MediaWeek(702, 429, 2, DateTime.Parse("Jun  5 2017 12:00AM"), DateTime.Parse("Jun 11 2017 12:00AM")),
            new MediaWeek(701, 429, 1, DateTime.Parse("May 29 2017 12:00AM"), DateTime.Parse("Jun  4 2017 12:00AM")),
            new MediaWeek(700, 428, 4, DateTime.Parse("May 22 2017 12:00AM"), DateTime.Parse("May 28 2017 12:00AM")),
            new MediaWeek(699, 428, 3, DateTime.Parse("May 15 2017 12:00AM"), DateTime.Parse("May 21 2017 12:00AM")),
            new MediaWeek(698, 428, 2, DateTime.Parse("May  8 2017 12:00AM"), DateTime.Parse("May 14 2017 12:00AM")),
            new MediaWeek(697, 428, 1, DateTime.Parse("May  1 2017 12:00AM"), DateTime.Parse("May  7 2017 12:00AM")),
            new MediaWeek(696, 427, 5, DateTime.Parse("Apr 24 2017 12:00AM"), DateTime.Parse("Apr 30 2017 12:00AM")),
            new MediaWeek(695, 427, 4, DateTime.Parse("Apr 17 2017 12:00AM"), DateTime.Parse("Apr 23 2017 12:00AM")),
            new MediaWeek(694, 427, 3, DateTime.Parse("Apr 10 2017 12:00AM"), DateTime.Parse("Apr 16 2017 12:00AM")),
            new MediaWeek(693, 427, 2, DateTime.Parse("Apr  3 2017 12:00AM"), DateTime.Parse("Apr  9 2017 12:00AM")),
            new MediaWeek(692, 427, 1, DateTime.Parse("Mar 27 2017 12:00AM"), DateTime.Parse("Apr  2 2017 12:00AM")),
            new MediaWeek(691, 426, 4, DateTime.Parse("Mar 20 2017 12:00AM"), DateTime.Parse("Mar 26 2017 12:00AM")),
            new MediaWeek(690, 426, 3, DateTime.Parse("Mar 13 2017 12:00AM"), DateTime.Parse("Mar 19 2017 12:00AM")),
            new MediaWeek(689, 426, 2, DateTime.Parse("Mar  6 2017 12:00AM"), DateTime.Parse("Mar 12 2017 12:00AM")),
            new MediaWeek(688, 426, 1, DateTime.Parse("Feb 27 2017 12:00AM"), DateTime.Parse("Mar  5 2017 12:00AM")),
            new MediaWeek(687, 425, 4, DateTime.Parse("Feb 20 2017 12:00AM"), DateTime.Parse("Feb 26 2017 12:00AM")),
            new MediaWeek(686, 425, 3, DateTime.Parse("Feb 13 2017 12:00AM"), DateTime.Parse("Feb 19 2017 12:00AM")),
            new MediaWeek(685, 425, 2, DateTime.Parse("Feb  6 2017 12:00AM"), DateTime.Parse("Feb 12 2017 12:00AM")),
            new MediaWeek(684, 425, 1, DateTime.Parse("Jan 30 2017 12:00AM"), DateTime.Parse("Feb  5 2017 12:00AM")),
            new MediaWeek(683, 424, 5, DateTime.Parse("Jan 23 2017 12:00AM"), DateTime.Parse("Jan 29 2017 12:00AM")),
            new MediaWeek(682, 424, 4, DateTime.Parse("Jan 16 2017 12:00AM"), DateTime.Parse("Jan 22 2017 12:00AM")),
            new MediaWeek(681, 424, 3, DateTime.Parse("Jan  9 2017 12:00AM"), DateTime.Parse("Jan 15 2017 12:00AM")),
            new MediaWeek(680, 424, 2, DateTime.Parse("Jan  2 2017 12:00AM"), DateTime.Parse("Jan  8 2017 12:00AM"))
        };

        private static List<MediaMonth> _MediaMonths = new List<MediaMonth>
        {
            new MediaMonth { Id = 424, Year = 2017 , Month = 1, MediaMonthX = "0117", StartDate = DateTime.Parse("2016-12-26"), EndDate = DateTime.Parse("2017-01-29") },
            new MediaMonth { Id = 425, Year = 2017 , Month = 2, MediaMonthX = "0217", StartDate = DateTime.Parse("2017-01-30"), EndDate = DateTime.Parse("2017-02-26") },
            new MediaMonth { Id = 426, Year = 2017 , Month = 3, MediaMonthX = "0317", StartDate = DateTime.Parse("2017-02-27"), EndDate = DateTime.Parse("2017-03-26") },
            new MediaMonth { Id = 427, Year = 2017 , Month = 4, MediaMonthX = "0417", StartDate = DateTime.Parse("2017-03-27"), EndDate = DateTime.Parse("2017-04-30") },
            new MediaMonth { Id = 428, Year = 2017 , Month = 5, MediaMonthX = "0517", StartDate = DateTime.Parse("2017-05-01"), EndDate = DateTime.Parse("2017-05-28") },
            new MediaMonth { Id = 429, Year = 2017 , Month = 6, MediaMonthX = "0617", StartDate = DateTime.Parse("2017-05-29"), EndDate = DateTime.Parse("2017-06-25") },
            new MediaMonth { Id = 430, Year = 2017 , Month = 7, MediaMonthX = "0717", StartDate = DateTime.Parse("2017-06-26"), EndDate = DateTime.Parse("2017-07-30") },
            new MediaMonth { Id = 431, Year = 2017 , Month = 8, MediaMonthX = "0817", StartDate = DateTime.Parse("2017-07-31"), EndDate = DateTime.Parse("2017-08-27") },
            new MediaMonth { Id = 432, Year = 2017 , Month = 9, MediaMonthX = "0917", StartDate = DateTime.Parse("2017-08-28"), EndDate = DateTime.Parse("2017-09-24") },
            new MediaMonth { Id = 433, Year = 2017 , Month = 10, MediaMonthX = "1017", StartDate = DateTime.Parse("2017-09-25"), EndDate = DateTime.Parse("2017-10-29") },
            new MediaMonth { Id = 434, Year = 2017 , Month = 11, MediaMonthX = "1117", StartDate = DateTime.Parse("2017-10-30"), EndDate = DateTime.Parse("2017-11-26") },
            new MediaMonth { Id = 435, Year = 2017 , Month = 12, MediaMonthX = "1217", StartDate = DateTime.Parse("2017-11-27"), EndDate = DateTime.Parse("2017-12-31") },
            new MediaMonth { Id = 436, Year = 2018 , Month = 1, MediaMonthX = "0118", StartDate = DateTime.Parse("2018-01-01"), EndDate = DateTime.Parse("2018-01-28") },
            new MediaMonth { Id = 437, Year = 2018 , Month = 2, MediaMonthX = "0218", StartDate = DateTime.Parse("2018-01-29"), EndDate = DateTime.Parse("2018-02-25") },
            new MediaMonth { Id = 438, Year = 2018 , Month = 3, MediaMonthX = "0318", StartDate = DateTime.Parse("2018-02-26"), EndDate = DateTime.Parse("2018-03-25") },
            new MediaMonth { Id = 439, Year = 2018 , Month = 4, MediaMonthX = "0418", StartDate = DateTime.Parse("2018-03-26"), EndDate = DateTime.Parse("2018-04-29") },
            new MediaMonth { Id = 440, Year = 2018 , Month = 5, MediaMonthX = "0518", StartDate = DateTime.Parse("2018-04-30"), EndDate = DateTime.Parse("2018-05-27") },
            new MediaMonth { Id = 441, Year = 2018 , Month = 6, MediaMonthX = "0618", StartDate = DateTime.Parse("2018-05-28"), EndDate = DateTime.Parse("2018-06-24") },
            new MediaMonth { Id = 442, Year = 2018 , Month = 7, MediaMonthX = "0718", StartDate = DateTime.Parse("2018-06-25"), EndDate = DateTime.Parse("2018-07-29") },
            new MediaMonth { Id = 443, Year = 2018 , Month = 8, MediaMonthX = "0818", StartDate = DateTime.Parse("2018-07-30"), EndDate = DateTime.Parse("2018-08-26") },
            new MediaMonth { Id = 444, Year = 2018 , Month = 9, MediaMonthX = "0918", StartDate = DateTime.Parse("2018-08-27"), EndDate = DateTime.Parse("2018-09-30") },
            new MediaMonth { Id = 445, Year = 2018 , Month = 10, MediaMonthX = "1018", StartDate = DateTime.Parse("2018-10-01"), EndDate = DateTime.Parse("2018-10-28") },
            new MediaMonth { Id = 446, Year = 2018 , Month = 11, MediaMonthX = "1118", StartDate = DateTime.Parse("2018-10-29"), EndDate = DateTime.Parse("2018-11-25") },
            new MediaMonth { Id = 447, Year = 2018 , Month = 12, MediaMonthX = "1218", StartDate = DateTime.Parse("2018-11-26"), EndDate = DateTime.Parse("2018-12-30") },
            new MediaMonth { Id = 448, Year = 2019 , Month = 1, MediaMonthX = "0119", StartDate = DateTime.Parse("2018-12-31"), EndDate = DateTime.Parse("2019-01-27") },
            new MediaMonth { Id = 449, Year = 2019 , Month = 2, MediaMonthX = "0219", StartDate = DateTime.Parse("2019-01-28"), EndDate = DateTime.Parse("2019-02-24") },
            new MediaMonth { Id = 450, Year = 2019 , Month = 3, MediaMonthX = "0319", StartDate = DateTime.Parse("2019-02-25"), EndDate = DateTime.Parse("2019-03-31") },
            new MediaMonth { Id = 451, Year = 2019 , Month = 4, MediaMonthX = "0419", StartDate = DateTime.Parse("2019-04-01"), EndDate = DateTime.Parse("2019-04-28") },
            new MediaMonth { Id = 452, Year = 2019 , Month = 5, MediaMonthX = "0519", StartDate = DateTime.Parse("2019-04-29"), EndDate = DateTime.Parse("2019-05-26") },
            new MediaMonth { Id = 453, Year = 2019 , Month = 6, MediaMonthX = "0619", StartDate = DateTime.Parse("2019-05-27"), EndDate = DateTime.Parse("2019-06-30") },
            new MediaMonth { Id = 454, Year = 2019 , Month = 7, MediaMonthX = "0719", StartDate = DateTime.Parse("2019-07-01"), EndDate = DateTime.Parse("2019-07-28") },
            new MediaMonth { Id = 455, Year = 2019 , Month = 8, MediaMonthX = "0819", StartDate = DateTime.Parse("2019-07-29"), EndDate = DateTime.Parse("2019-08-25") },
            new MediaMonth { Id = 456, Year = 2019 , Month = 9, MediaMonthX = "0919", StartDate = DateTime.Parse("2019-08-26"), EndDate = DateTime.Parse("2019-09-29") },
            new MediaMonth { Id = 457, Year = 2019 , Month = 10, MediaMonthX = "1019", StartDate = DateTime.Parse("2019-09-30"), EndDate = DateTime.Parse("2019-10-27") },
            new MediaMonth { Id = 458, Year = 2019 , Month = 11, MediaMonthX = "1119", StartDate = DateTime.Parse("2019-10-28"), EndDate = DateTime.Parse("2019-11-24") },
            new MediaMonth { Id = 459, Year = 2019 , Month = 12, MediaMonthX = "1219", StartDate = DateTime.Parse("2019-11-25"), EndDate = DateTime.Parse("2019-12-29") },
            new MediaMonth { Id = 460, Year = 2020 , Month = 1, MediaMonthX = "0120", StartDate = DateTime.Parse("2019-12-30"), EndDate = DateTime.Parse("2020-01-26") },
            new MediaMonth { Id = 461, Year = 2020 , Month = 2, MediaMonthX = "0220", StartDate = DateTime.Parse("2020-01-27"), EndDate = DateTime.Parse("2020-02-23") },
            new MediaMonth { Id = 462, Year = 2020 , Month = 3, MediaMonthX = "0320", StartDate = DateTime.Parse("2020-02-24"), EndDate = DateTime.Parse("2020-03-29") },
            new MediaMonth { Id = 463, Year = 2020 , Month = 4, MediaMonthX = "0420", StartDate = DateTime.Parse("2020-03-30"), EndDate = DateTime.Parse("2020-04-26") },
            new MediaMonth { Id = 464, Year = 2020 , Month = 5, MediaMonthX = "0520", StartDate = DateTime.Parse("2020-04-27"), EndDate = DateTime.Parse("2020-05-31") },
            new MediaMonth { Id = 465, Year = 2020 , Month = 6, MediaMonthX = "0620", StartDate = DateTime.Parse("2020-06-01"), EndDate = DateTime.Parse("2020-06-28") },
            new MediaMonth { Id = 466, Year = 2020 , Month = 7, MediaMonthX = "0720", StartDate = DateTime.Parse("2020-06-29"), EndDate = DateTime.Parse("2020-07-26") },
            new MediaMonth { Id = 467, Year = 2020 , Month = 8, MediaMonthX = "0820", StartDate = DateTime.Parse("2020-07-27"), EndDate = DateTime.Parse("2020-08-30") },
            new MediaMonth { Id = 468, Year = 2020 , Month = 9, MediaMonthX = "0920", StartDate = DateTime.Parse("2020-08-31"), EndDate = DateTime.Parse("2020-09-27") },
            new MediaMonth { Id = 469, Year = 2020 , Month = 10, MediaMonthX = "1020", StartDate = DateTime.Parse("2020-09-28"), EndDate = DateTime.Parse("2020-10-25") },
            new MediaMonth { Id = 470, Year = 2020 , Month = 11, MediaMonthX = "1120", StartDate = DateTime.Parse("2020-10-26"), EndDate = DateTime.Parse("2020-11-29") },
            new MediaMonth { Id = 471, Year = 2020 , Month = 12, MediaMonthX = "1220", StartDate = DateTime.Parse("2020-11-30"), EndDate = DateTime.Parse("2020-12-27") },
            new MediaMonth { Id = 472, Year = 2021 , Month = 1, MediaMonthX = "0121", StartDate = DateTime.Parse("2020-12-28"), EndDate = DateTime.Parse("2021-01-31") },
            new MediaMonth { Id = 473, Year = 2021 , Month = 2, MediaMonthX = "0221", StartDate = DateTime.Parse("2021-02-01"), EndDate = DateTime.Parse("2021-02-28") },
            new MediaMonth { Id = 474, Year = 2021 , Month = 3, MediaMonthX = "0321", StartDate = DateTime.Parse("2021-03-01"), EndDate = DateTime.Parse("2021-03-28") },
            new MediaMonth { Id = 475, Year = 2021 , Month = 4, MediaMonthX = "0421", StartDate = DateTime.Parse("2021-03-29"), EndDate = DateTime.Parse("2021-04-25") },
            new MediaMonth { Id = 476, Year = 2021 , Month = 5, MediaMonthX = "0521", StartDate = DateTime.Parse("2021-04-26"), EndDate = DateTime.Parse("2021-05-30") },
            new MediaMonth { Id = 477, Year = 2021 , Month = 6, MediaMonthX = "0621", StartDate = DateTime.Parse("2021-05-31"), EndDate = DateTime.Parse("2021-06-27") },
            new MediaMonth { Id = 478, Year = 2021 , Month = 7, MediaMonthX = "0721", StartDate = DateTime.Parse("2021-06-28"), EndDate = DateTime.Parse("2021-07-25") },
            new MediaMonth { Id = 479, Year = 2021 , Month = 8, MediaMonthX = "0821", StartDate = DateTime.Parse("2021-07-26"), EndDate = DateTime.Parse("2021-08-29") },
            new MediaMonth { Id = 480, Year = 2021 , Month = 9, MediaMonthX = "0921", StartDate = DateTime.Parse("2021-08-30"), EndDate = DateTime.Parse("2021-09-26") },
            new MediaMonth { Id = 481, Year = 2021 , Month = 10, MediaMonthX = "1021", StartDate = DateTime.Parse("2021-09-27"), EndDate = DateTime.Parse("2021-10-31") },
            new MediaMonth { Id = 482, Year = 2021 , Month = 11, MediaMonthX = "1121", StartDate = DateTime.Parse("2021-11-01"), EndDate = DateTime.Parse("2021-11-28") },
            new MediaMonth { Id = 483, Year = 2021 , Month = 12, MediaMonthX = "1221", StartDate = DateTime.Parse("2021-11-29"), EndDate = DateTime.Parse("2021-12-26") },

        };

        #endregion // #region Big Lists
    }
}