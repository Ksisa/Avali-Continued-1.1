using System;
using Verse;
using Verse.AI;

// AvaliUtility.FindPawnsForLovinInRoom(this Pawn pawn, Room room, bool giveThought = false):
// {
//		return null если эта пешка снаружи
// 		Найти всех пешек в комнате
// 		return null если любая пешка в комнате без сознания или мертва
// 		return null если любая Humanlike пешка в комнате не пренадлежит к паку и имеет меньше minOpinion отношений с любым из членов пака
//		Отфильтровать пешек по трейтам
// }

// JobGiver_GotoForLovinAvaliGroup:
// {
//		Первичные проверки
//		return null если FindPawnsForLovin(false) == null
//		Дать работу JobDriver_LovinAvali()
// }

// JobDriver_FindPartnerForLovinAvali:
// {
//		Если FindPawnsForLovin(false) != null
// 		Из них найти себе наиболее подходящего партнера
//		Если партнер занят то искать любого другого партнера удовлетворяющего всем параметрам
//		Если есть свободный партнер удовлетворяющий всем параметрам то найти кровать партнера в комнате, 
//		если кровать null или занята то искать свою кровать в комнате,
//		если своя кровать не null и не занята, иначе найти в комнате объект с максимальным значением комфорта из всех объектов в комнате
//		Если нет свободных партнеров удовлетворяющих всем параметрам то встать в очередь к наиболее подходящеему партнеру
//		StartJob JobDriver_GotoForLovinAvali
// }

// JobDriver_GotoForLovinAvali:
// {
//		Идти к партнеру
//		Если FindPawnsForLovin(false) != null
//		Если партнер не свободен то ждать, изменить название работы на waiting или watching (50/50).	
//		Если партнер свободен StartJob JobDriver_LovinAvali
// }

// JobDriver_LovinAvali:
// {
//		Зарезервировать партнера и кровать(если имеется)
// 		Идти к партнеру
//		Если FindPawnsForLovin(false) != null
//		Mote "Mote_HeartSpeech" для себя, ждать 200 тиков, mote "Mote_HeartSpeech" для партнера
//		Идти к кровати/объекту вместе с партнером
//		Если FindPawnsForLovin(false) != null
//		Mote "Mote_HeartSpeech" для себя и партнера
//	 	Lovin (выполнять каждый тик)
//		{
//			Получить Comfort (self and partner)
//			Получить Joy(Social) (self and partner)
//			
//			Выполнять каждый указанный интервал:
//			EndJob если FindPawnsForLovin(true) == null
//			Попытаться дать всем незянятым(или занятым работой типа joy) здоровым пешкам в списке работу JobDriver_FindPartnerForLovinAvali()
//		}
//		Give GotSomeLovin thought
//		Обновить HoursToNextLovin в Hediff_AvaliBiology
//		С вероятностью 25% изменить название работы на watching
//		EndJob
// }

namespace Avali
{
	public class JobGiver_GotoForLovinAvaliGroup : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.def != ThingDefOf.Avali || Find.TickManager.TicksGame < pawn.mindState.canLovinTick)
			{
				/*
				string inventory = pawn.inventory.ToString();
				if (inventory.Contains("EggAvali"))
				{
					
				}
				*/
				
				//Log.Message(TickManager.TicksGame + "(TickManager.TicksGame) + " < " + pawn.mindState.canLovinTick + "(pawn.mindState.canLovinTick)");
				return null;
			}
			
			return new Job(JobDefOf.FindPartnerForLovinAvali);
		}
	}
}
